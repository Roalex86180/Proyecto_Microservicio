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
using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.Functions
{
    public class ProcessPayment
    {
        private readonly ILogger<ProcessPayment> _logger;
        private readonly CosmosClient _reviewsClient;
        private readonly CosmosClient _cartClient;

        public ProcessPayment(ILogger<ProcessPayment> logger, Func<string, CosmosClient> clientFactory)
        {
            _logger = logger;
            _reviewsClient = clientFactory("ReviewsDbClient");
            _cartClient = clientFactory("CartDbClient");
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
                // Flujo de pago directo
                if (!string.IsNullOrEmpty(paymentRequest.CourseId))
                {
                    _logger.LogInformation($"Processing direct payment for course '{paymentRequest.CourseId}' for user '{paymentRequest.UserId}'.");

                    if (string.IsNullOrEmpty(paymentRequest.ProductName) || !paymentRequest.Price.HasValue)
                    {
                        var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                        await errorResponse.WriteStringAsync("For a direct payment, ProductName and Price are required in the request body.");
                        return errorResponse;
                    }

                    // Se crea un registro de compra en la base de datos de reseñas (ReviewsDb)
                    var reviewsDatabase = _reviewsClient.GetDatabase("ReviewsDb");
                    var reviewsContainer = reviewsDatabase.GetContainer("Reviews");

                    var purchaseRecord = new UserPurchase
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = paymentRequest.UserId,
                        ProductId = paymentRequest.CourseId,
                        ProductName = paymentRequest.ProductName,
                        Price = paymentRequest.Price.Value,
                        PurchaseDate = DateTime.UtcNow
                    };

                    await reviewsContainer.CreateItemAsync(purchaseRecord, new PartitionKey(purchaseRecord.UserId));
                    _logger.LogInformation($"Course '{purchaseRecord.ProductName}' permanently saved to UserPurchases for user '{purchaseRecord.UserId}'.");

                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteAsJsonAsync(new
                    {
                        message = $"Payment for course '{paymentRequest.CourseId}' processed successfully."
                    });
                    return successResponse;
                }
                // Flujo de pago del carrito
                else
                {
                    var cartDatabase = _cartClient.GetDatabase("CartDb");
                    var cartContainer = cartDatabase.GetContainer("Items"); // Contenedor para leer y eliminar items del carrito

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

                    // CORRECCIÓN: Usamos el contenedor de compras en la base de datos del carrito
                    var purchasesContainer = cartDatabase.GetContainer("Purchases"); // Contenedor para registrar las compras

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

                    if (!purchaseBatchResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Transactional batch for UserPurchases failed with status code: {purchaseBatchResponse.StatusCode}.");
                        
                        for (int i = 0; i < purchaseBatchResponse.Count; i++)
                        {
                            var operation = purchaseBatchResponse.GetOperationResultAtIndex<UserPurchase>(i);
                            if (!operation.IsSuccessStatusCode)
                            {
                                _logger.LogError($"Operation at index {i} failed with status code: {operation.StatusCode}.");
                            }
                        }

                        var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                        await errorResponse.WriteStringAsync("Payment processed, but failed to record purchases. Please contact support.");
                        return errorResponse;
                    }

                    _logger.LogInformation($"Successfully recorded {cartItems.Count} new purchases for user '{paymentRequest.UserId}'.");

                    // CORRECCIÓN: Usamos un segundo batch transaccional para limpiar el carrito
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