using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using AuthenticationService.Models;
using System.Linq;
using System;

namespace AuthenticationService.Functions
{
    public class DeleteFunction
    {
        private readonly ILogger<DeleteFunction> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseName;
        private readonly string _containerName;

        public DeleteFunction(ILogger<DeleteFunction> logger, IConfiguration configuration, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _databaseName = configuration.GetValue<string>("CosmosDbDatabaseName") ?? throw new InvalidOperationException("CosmosDbDatabaseName is not configured.");
            _containerName = configuration.GetValue<string>("CosmosDbContainerName") ?? throw new InvalidOperationException("CosmosDbContainerName is not configured.");
        }

        [Function("Delete")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "delete")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function 'Delete' processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                var deleteRequest = JsonSerializer.Deserialize<DeleteRequest>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (string.IsNullOrEmpty(deleteRequest?.Username))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Please pass a 'username' in the request body." }));
                    return badRequestResponse;
                }

                var container = _cosmosClient.GetContainer(_databaseName, _containerName);

                var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @username")
                    .WithParameter("@username", deleteRequest.Username);
                
                // Corregido: Especificación completa del tipo User
                using var feedIterator = container.GetItemQueryIterator<AuthenticationService.Models.User>(query);
                var user = (await feedIterator.ReadNextAsync()).FirstOrDefault();
                
                if (user == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = $"User '{deleteRequest.Username}' not found." }));
                    return notFoundResponse;
                }

                var partitionKey = new PartitionKey(user.Username);
                // Corregido: Especificación completa del tipo User
                await container.DeleteItemAsync<AuthenticationService.Models.User>(user.Id, partitionKey);

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteStringAsync(JsonSerializer.Serialize(new { message = $"La anulación de la cuenta '{deleteRequest.Username}' fue satisfactoria." }));
                return response;
            }
            catch (JsonException)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Invalid JSON format." }));
                return badRequestResponse;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "User not found." }));
                return notFoundResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user deletion.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "An unexpected error occurred." }));
                return errorResponse;
            }
        }
    }
}