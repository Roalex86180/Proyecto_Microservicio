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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using Microsoft.Extensions.Configuration;

namespace AuthenticationService.Functions
{
    public class LoginFunction
    {
        private readonly ILogger<LoginFunction> _logger;
        private readonly Container _container;
        private readonly IConfiguration _configuration;

        public LoginFunction(ILogger<LoginFunction> logger, CosmosClient cosmosClient, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            
            var databaseName = configuration.GetValue<string>("CosmosDbDatabaseName");
            var containerName = configuration.GetValue<string>("CosmosDbContainerName");
            
            _container = cosmosClient.GetDatabase(databaseName).GetContainer(containerName);
        }

        [Function("Login")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "login")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP trigger function 'Login' processed a request.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                
                var loginRequest = JsonSerializer.Deserialize<LoginRequest>(requestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (string.IsNullOrEmpty(loginRequest?.Username) || string.IsNullOrEmpty(loginRequest?.Password))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Username and password are required." }));
                    return badRequestResponse;
                }

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.username = @username")
                    .WithParameter("@username", loginRequest.Username);

                // Corregido: Especificación completa del tipo User
                AuthenticationService.Models.User? user = null;
                using (var feedIterator = _container.GetItemQueryIterator<AuthenticationService.Models.User>(queryDefinition))
                {
                    var response = await feedIterator.ReadNextAsync();
                    user = response.FirstOrDefault();
                }

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login failed for user '{loginRequest.Username}' due to invalid credentials.");
                    var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                    await unauthorizedResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Invalid credentials." }));
                    return unauthorizedResponse;
                }

                var jwtSecret = _configuration.GetValue<string>("JwtSecretKey");
                if (string.IsNullOrEmpty(jwtSecret))
                {
                    _logger.LogError("JwtSecretKey is not configured.");
                    var internalServerErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await internalServerErrorResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "An unexpected error occurred." }));
                    return internalServerErrorResponse;
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("email", user.Email)
                };

                var token = new JwtSecurityToken(
                    issuer: "AuthenticationService",
                    audience: "CourseCatalogService",
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: credentials);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation($"User '{user.Username}' logged in successfully.");

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                successResponse.Headers.Add("Content-Type", "application/json");
                await successResponse.WriteStringAsync(JsonSerializer.Serialize(new { token = tokenString }));
                return successResponse;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error during user login.");
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "Invalid JSON format." }));
                return badRequestResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during user login.");
                var internalServerErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await internalServerErrorResponse.WriteStringAsync(JsonSerializer.Serialize(new { message = "An unexpected error occurred." }));
                return internalServerErrorResponse;
            }
        }
    }
}