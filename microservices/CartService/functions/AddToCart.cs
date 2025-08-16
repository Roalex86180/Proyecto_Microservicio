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
    public class AddToCart
    {
        private readonly ILogger<AddToCart> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public AddToCart(ILogger<AddToCart> logger, CosmosClient cosmosClient, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _httpClientFactory = httpClientFactory;
        }

        [Function("AddToCart")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add")] HttpRequestData req)
        {
            _logger.LogInformation("Add to cart request received.");

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

            CartItemRequest? cartItemRequest = null;
            try
            {
                cartItemRequest = JsonSerializer.Deserialize<CartItemRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing request body.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Invalid JSON format.");
                return errorResponse;
            }

            if (cartItemRequest == null || string.IsNullOrEmpty(cartItemRequest.ProductId))
            {
                var response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid cart item data. ProductId is required.");
                return response;
            }

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                
                var courseCatalogServiceUrl = Environment.GetEnvironmentVariable("CourseCatalogServiceUrl");
                _logger.LogInformation($"Calling Course Catalog Service at: {courseCatalogServiceUrl}/api/courses/{cartItemRequest.ProductId}");

                var courseResponse = await httpClient.GetAsync($"{courseCatalogServiceUrl}/api/courses/{cartItemRequest.ProductId}");

                if (courseResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Course with ID '{cartItemRequest.ProductId}' not found.");
                    return notFoundResponse;
                }

                courseResponse.EnsureSuccessStatusCode();

                var courseContent = await courseResponse.Content.ReadAsStringAsync();
                var course = JsonSerializer.Deserialize<Course>(courseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (course == null || string.IsNullOrEmpty(course.Name))
                {
                    var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await errorResponse.WriteStringAsync("Course data from catalog is invalid.");
                    return errorResponse;
                }

                var userId = cartItemRequest.UserId;

                var database = _cosmosClient.GetDatabase("CartDb");
                var container = database.GetContainer("Items");

                // Buscamos si el item ya existe para este usuario y product
                var queryable = container.GetItemLinqQueryable<CartItem>();
                using var existingItemQuery = queryable.Where(item => item.UserId == userId && item.ProductId == cartItemRequest.ProductId)
                                                       .Take(1)
                                                       .ToFeedIterator();
                var existingCartItem = (await existingItemQuery.ReadNextAsync()).FirstOrDefault();

                if (existingCartItem != null)
                {
                    // Si existe, devolvemos un error
                    _logger.LogInformation($"Item '{existingCartItem.ProductName}' already exists in cart for user '{userId}'.");
                    var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    await conflictResponse.WriteStringAsync($"Course '{existingCartItem.ProductName}' is already in your cart.");
                    return conflictResponse;
                }
                else
                {
                    // Si no existe, creamos un nuevo item
                    var cartItem = new CartItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        ProductId = cartItemRequest.ProductId,
                        ProductName = course.Name,
                        Price = course.Price,
                        Quantity = cartItemRequest.Quantity
                    };
                    await container.CreateItemAsync(cartItem, new PartitionKey(cartItem.UserId));
                    
                    _logger.LogInformation($"New item '{cartItem.ProductName}' added to Cosmos DB for user '{cartItem.UserId}' with ID: {cartItem.Id}");
                    
                    var successResponse = req.CreateResponse(HttpStatusCode.Created);
                    await successResponse.WriteAsJsonAsync(new
                    {
                        message = $"New item '{cartItem.ProductName}' added to cart successfully.",
                        id = cartItem.Id
                    });
                    return successResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing the request or saving to Cosmos DB.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error processing the request.");
                return errorResponse;
            }
        }
    }
}