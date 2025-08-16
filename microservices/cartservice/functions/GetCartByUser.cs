using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using CartService.Models;
using System.IO;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;

namespace CartService.Functions
{
    public class GetCartByUser
    {
        private readonly ILogger<GetCartByUser> _logger;
        private readonly CosmosClient _cosmosClient;

        public GetCartByUser(ILogger<GetCartByUser> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        [Function("GetCartByUser")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cart/{userId}")] HttpRequestData req,
            string userId)
        {
            _logger.LogInformation($"Get cart request received for user ID: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("User ID is required.");
                return badRequestResponse;
            }

            try
            {
                var database = _cosmosClient.GetDatabase("CartDb");
                var container = database.GetContainer("Items");

                // Consulta Cosmos DB para obtener todos los ítems del carrito para el usuario dado.
                var queryable = container.GetItemLinqQueryable<CartItem>();
                using var feedIterator = queryable.Where(item => item.UserId == userId).ToFeedIterator();

                var cartItems = new List<CartItem>();
                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    cartItems.AddRange(response.ToList());
                }

                _logger.LogInformation($"Found {cartItems.Count} items in cart for user '{userId}'.");

                // Devuelve los ítems del carrito como una respuesta JSON.
                // Si no se encuentran ítems, devolverá un array JSON vacío.
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(cartItems);
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cart for user ID: {userId}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error.");
                return errorResponse;
            }
        }
    }
}