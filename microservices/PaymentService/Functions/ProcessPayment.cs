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
                var database = _cosmosClient.GetDatabase("CartDb");
                var container = database.GetContainer("Items");

                // 1. Obtener todos los ítems del carrito del usuario usando la clave de partición
                _logger.LogInformation($"Searching for cart items for UserId: {paymentRequest.UserId}");

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                    .WithParameter("@userId", paymentRequest.UserId);
                
                var cartItems = new List<CartItem>();
                using (var feedIterator = container.GetItemQueryIterator<CartItem>(queryDefinition, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(paymentRequest.UserId) }))
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

                // 2. Simular el pago
                _logger.LogInformation($"Simulating payment for user '{paymentRequest.UserId}' with {cartItems.Count} items.");
                
                // 3. Eliminar los ítems del carrito usando un lote transaccional
                var batch = container.CreateTransactionalBatch(new PartitionKey(paymentRequest.UserId));
                foreach (var item in cartItems)
                {
                    batch.DeleteItem(item.Id);
                }

                var batchResponse = await batch.ExecuteAsync();

                if (!batchResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Transactional batch failed with status code: {batchResponse.StatusCode}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Payment processed, but failed to clear cart. Please contact support.");
                    return errorResponse;
                }

                _logger.LogInformation($"Successfully processed payment and cleared cart for user '{paymentRequest.UserId}'.");

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(new
                {
                    message = $"Payment processed successfully. {cartItems.Count} items removed from cart."
                });
                return successResponse;
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