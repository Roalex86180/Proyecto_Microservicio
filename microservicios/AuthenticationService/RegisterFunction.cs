using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AuthenticationService.Models;
using System.IO;
using BCrypt.Net;
using System.Linq;
using System;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;

namespace AuthenticationService.Functions
{
    public class RegisterFunction
    {
        private readonly ILogger<RegisterFunction> _logger;
        private readonly Container _container;
        private readonly IConfiguration _configuration;

        public RegisterFunction(ILogger<RegisterFunction> logger, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var databaseName = _configuration.GetValue<string>("CosmosDbDatabaseName");
            var containerName = _configuration.GetValue<string>("CosmosDbContainerName");
            
            _container = cosmosClient.GetDatabase(databaseName).GetContainer(containerName);
        }

        [Function("Register")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "register")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP trigger function 'Register' processed a request.");
            
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation($"Request body: {requestBody}");
                
                var registerRequest = JsonSerializer.Deserialize<RegisterRequest>(requestBody, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (registerRequest == null)
                {
                    _logger.LogWarning("Failed to deserialize request body");
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Invalid request format." }));
                    return badRequestResponse;
                }

                if (string.IsNullOrEmpty(registerRequest.Email) || 
                    string.IsNullOrEmpty(registerRequest.Password) || 
                    string.IsNullOrEmpty(registerRequest.Username))
                {
                    _logger.LogWarning("Missing required fields in registration request");
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    badRequestResponse.Headers.Add("Content-Type", "application/json");
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Username, email, and password are required." }));
                    return badRequestResponse;
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.username = @username")
                    .WithParameter("@username", registerRequest.Username);
                
                // Corregido: Especificación completa del tipo User
                var existingUserQuery = _container.GetItemQueryIterator<AuthenticationService.Models.User>(queryDefinition);
                
                // Corregido: Especificación completa del tipo User
                AuthenticationService.Models.User? existingUser = null;
                if (existingUserQuery.HasMoreResults)
                {
                    var queryResponse = await existingUserQuery.ReadNextAsync();
                    existingUser = queryResponse.FirstOrDefault();
                }

                // El error CS0019 se resuelve porque ahora el compilador conoce el tipo de 'existingUser'
                if (existingUser != null)
                {
                    _logger.LogWarning($"Registration attempt for existing user: {registerRequest.Username}");
                    var conflictResponse = req.CreateResponse(HttpStatusCode.Conflict);
                    conflictResponse.Headers.Add("Content-Type", "application/json");
                    await conflictResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = $"User with username '{registerRequest.Username}' already exists." }));
                    return conflictResponse;
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);
                
                // Corregido: Especificación completa del tipo User
                var userToRegister = new AuthenticationService.Models.User
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = passwordHash
                };
                
                // Corregido: Especificación completa del tipo User
                ItemResponse<AuthenticationService.Models.User> creationResponse = await _container.CreateItemAsync(
                    userToRegister, 
                    new PartitionKey(userToRegister.Username)
                );
                
                _logger.LogInformation($"User '{userToRegister.Username}' registered successfully. Status: {creationResponse.StatusCode}");
                
                try
                {
                    string? storageConnectionString = _configuration.GetValue<string>("AzureWebJobsStorage");
                    if (!string.IsNullOrEmpty(storageConnectionString))
                    {
                        string queueName = "user-registrations";
                        var queueClient = new QueueClient(storageConnectionString, queueName);
                        await queueClient.CreateIfNotExistsAsync();
                        
                        var messagePayload = new { userToRegister.Username, userToRegister.Email };
                        string messageJson = JsonSerializer.Serialize(messagePayload);
                        
                        _logger.LogInformation("Sending message to queue: {queueName}", queueName);
                        await queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson)));
                    }
                    else
                    {
                        _logger.LogWarning("AzureWebJobsStorage connection string not found. Skipping queue message.");
                    }
                }
                catch (Exception queueEx)
                {
                    _logger.LogWarning(queueEx, "Failed to send message to queue, but user registration succeeded");
                }

                var successResponse = req.CreateResponse(HttpStatusCode.Created);
                successResponse.Headers.Add("Content-Type", "application/json");
                await successResponse.WriteStringAsync(JsonSerializer.Serialize(new 
                { 
                    message = "User registered successfully.",
                    userId = userToRegister.Id,
                    email = userToRegister.Email
                }));
                return successResponse;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error during user registration");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                badRequestResponse.Headers.Add("Content-Type", "application/json");
                await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Invalid JSON format." }));
                return badRequestResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user registration");
                var internalServerErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                internalServerErrorResponse.Headers.Add("Content-Type", "application/json");
                await internalServerErrorResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "An unexpected error occurred." }));
                return internalServerErrorResponse;
            }
        }
    }
}