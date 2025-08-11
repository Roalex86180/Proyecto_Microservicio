using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AuthenticationService.Functions
{
    public class TestCosmosFunction
    {
        private readonly ILogger<TestCosmosFunction> _logger;
        private readonly Container _container;
        private readonly IConfiguration _configuration;

        public TestCosmosFunction(ILogger<TestCosmosFunction> logger, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var databaseName = _configuration.GetValue<string>("CosmosDbDatabaseName");
            var containerName = _configuration.GetValue<string>("CosmosDbContainerName");
            
            _logger.LogInformation($"Database: {databaseName}, Container: {containerName}");
            
            _container = cosmosClient.GetDatabase(databaseName).GetContainer(containerName);
        }

        [Function("TestCosmos")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "test-cosmos")] HttpRequestData req)
        {
            _logger.LogInformation("Testing Cosmos DB connection...");
            
            try
            {
                // Intentar hacer una query simple
                var queryDefinition = new QueryDefinition("SELECT TOP 1 * FROM c");
                var queryIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);
                
                _logger.LogInformation("Query created successfully");
                
                if (queryIterator.HasMoreResults)
                {
                    var response = await queryIterator.ReadNextAsync();
                    _logger.LogInformation($"Query executed successfully. Items count: {response.Count}");
                }
                
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                successResponse.Headers.Add("Content-Type", "application/json");
                await successResponse.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    message = "Cosmos DB connection successful!",
                    database = _configuration.GetValue<string>("CosmosDbDatabaseName"),
                    container = _configuration.GetValue<string>("CosmosDbContainerName")
                }));
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cosmos DB connection failed");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                errorResponse.Headers.Add("Content-Type", "application/json");
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    message = "Cosmos DB connection failed",
                    error = ex.Message,
                    type = ex.GetType().Name
                }));
                return errorResponse;
            }
        }
    }
}