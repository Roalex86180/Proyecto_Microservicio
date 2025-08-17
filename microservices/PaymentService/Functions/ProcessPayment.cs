// src/PaymentService/Functions/ProcessPayment.cs

using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using PaymentService.Models;
using CartService.Models;
using System.Collections.Generic;

namespace PaymentService.Functions
{
    public class ProcessPayment
    {
        private readonly ILogger<ProcessPayment> _logger;
        private readonly CosmosClient _cosmosClient;

        public ProcessPayment(ILogger<ProcessPayment> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        [Function("ProcessPayment")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "payment")] HttpRequestData req)
        {
            _logger.LogInformation("Payment request received.");

            string requestBody;
            try
            {
                requestBody = await req.ReadAsStringAsync() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Invalid request body.");
                return errorResponse;
            }

            PaymentRequest? paymentRequest = null;
            try
            {
                paymentRequest = JsonSerializer.Deserialize<PaymentRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Invalid JSON format.");
                return errorResponse;
            }

            if (paymentRequest == null || string.IsNullOrEmpty(paymentRequest.UserId))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid payment data. UserId is required.");
                return response;
            }

            try
            {
                var cartDatabase = _cosmosClient.GetDatabase("CartDb");
                var cartContainer = cartDatabase.GetContainer("Items");

                // [MODIFICADO] Obtenemos la referencia de la nueva base de datos PurchasesDb
                var purchasesDatabase = _cosmosClient.GetDatabase("PurchasesDb");
                var purchasesContainer = purchasesDatabase.GetContainer("UserPurchases");

                // Lógica para procesar un solo curso si se especifica
                if (!string.IsNullOrEmpty(paymentRequest.CourseId))
                {
                    _logger.LogInformation($"Processing single course '{paymentRequest.CourseId}' for user '{paymentRequest.UserId}'.");
                    
                    var querySingleItem = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId AND c.productId = @courseId")
                        .WithParameter("@userId", paymentRequest.UserId)
                        .WithParameter("@courseId", paymentRequest.CourseId);

                    var singleItem = (await cartContainer.GetItemQueryIterator<CartItem>(querySingleItem, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(paymentRequest.UserId) })
                                                         .ReadNextAsync()).FirstOrDefault();
                    
                    if (singleItem != null)
                    {
                        // [NUEVA LÓGICA] 1. Guardar el ítem en la nueva colección permanente
                        var purchaseRecord = new UserPurchase
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = singleItem.UserId,
                            ProductId = singleItem.ProductId,
                            ProductName = singleItem.ProductName,
                            Price = singleItem.Price,
                            PurchaseDate = DateTime.UtcNow
                        };
                        await purchasesContainer.CreateItemAsync(purchaseRecord, new PartitionKey(purchaseRecord.UserId));
                        _logger.LogInformation($"Course '{singleItem.ProductName}' permanently saved to UserPurchases for user '{singleItem.UserId}'.");
                        
                        // [LÓGICA EXISTENTE] 2. Eliminar el ítem del carrito temporal
                        await cartContainer.DeleteItemAsync<CartItem>(singleItem.Id, new PartitionKey(paymentRequest.UserId));
                    }

                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteAsJsonAsync(new
                    {
                        message = $"Payment for course '{paymentRequest.CourseId}' processed successfully."
                    });
                    return successResponse;
                }
                else // Lógica existente para procesar el carrito completo
                {
                    _logger.LogInformation($"Searching for cart items for UserId: {paymentRequest.UserId}");

                    var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                        .WithParameter("@userId", paymentRequest.UserId);
                    
                    var cartItems = new List<CartItem>();
                    using (var feedIterator = cartContainer.GetItemQueryIterator<CartItem>(queryDefinition, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(paymentRequest.UserId) }))
                    {
                        while (feedIterator.HasMoreResults)
                        {
                            var response = await feedIterator.ReadNextAsync();
                            cartItems.AddRange(response);
                        }
                    }

                    if (cartItems.Count == 0)
                    {
                        var response = req.CreateResponse(HttpStatusCode.BadRequest);
                        await response.WriteStringAsync($"Cart for user '{paymentRequest.UserId}' is empty. No payment to process.");
                        return response;
                    }
                    
                    _logger.LogInformation($"Simulating payment for user '{paymentRequest.UserId}' with {cartItems.Count} items.");
                    
                    // [NUEVA LÓGICA] 1. Guardar todos los ítems en la nueva colección permanente
                    var purchaseBatch = purchasesContainer.CreateTransactionalBatch(new PartitionKey(paymentRequest.UserId));
                    foreach (var item in cartItems)
                    {
                        var purchaseRecord = new UserPurchase
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = item.UserId,
                            ProductId = item.ProductId,
                            ProductName = item.ProductName,
                            Price = item.Price,
                            PurchaseDate = DateTime.UtcNow
                        };
                        purchaseBatch.CreateItem(purchaseRecord);
                    }
                    var purchaseBatchResponse = await purchaseBatch.ExecuteAsync();

                    // [INICIO DEL CAMBIO] Lógica de logging detallado CORRECTA
                    if (!purchaseBatchResponse.IsSuccessStatusCode)
                    {
                        // Logueamos el código de estado del lote principal
                        _logger.LogError($"Transactional batch for UserPurchases failed with status code: {purchaseBatchResponse.StatusCode}.");
                        
                        // Iteramos a través de cada operación para encontrar la que falló
                        for (int i = 0; i < purchaseBatchResponse.Count; i++)
                        {
                            var operation = purchaseBatchResponse.GetOperationResultAtIndex(i);
                            if (!operation.IsSuccessStatusCode)
                            {
                                _logger.LogError($"Operation at index {i} failed with status code: {operation.StatusCode}.");
                            }
                        }
                        
                        var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                        await errorResponse.WriteStringAsync("Payment processed, but failed to record purchases. Please contact support.");
                        return errorResponse;
                    }
                    // [FIN DEL CAMBIO]

                    _logger.LogInformation($"Successfully recorded {cartItems.Count} new purchases for user '{paymentRequest.UserId}'.");

                    // [LÓGICA EXISTENTE] 2. Eliminar todos los ítems del carrito temporal
                    var cartBatch = cartContainer.CreateTransactionalBatch(new PartitionKey(paymentRequest.UserId));
                    foreach (var item in cartItems)
                    {
                        cartBatch.DeleteItem(item.Id);
                    }
                    var cartBatchResponse = await cartBatch.ExecuteAsync();

                    if (!cartBatchResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Transactional batch failed with status code: {cartBatchResponse.StatusCode}");
                        var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                        await errorResponse.WriteStringAsync("Payment processed, but failed to clear cart. Please contact support.");
                        return errorResponse;
                    }
                    
                    _logger.LogInformation($"Successfully processed payment and cleared cart for user '{paymentRequest.UserId}'.");

                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteAsJsonAsync(new
                    {
                        message = $"Pago realizado con éxito. {cartItems.Count} ítem{(cartItems.Count > 1 ? "s" : "")} eliminad{(cartItems.Count > 1 ? "os" : "o")} del carrito."
                    });
                    return successResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the payment.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error processing the payment.");
                return errorResponse;
            }
        }
    }
}