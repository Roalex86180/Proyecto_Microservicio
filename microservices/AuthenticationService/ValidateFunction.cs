using System;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Extensions.Configuration; // Nuevo using para la configuración

namespace AuthenticationService.Functions
{
    public class ValidateFunction
    {
        private readonly ILogger<ValidateFunction> _logger;
        private readonly IConfiguration _configuration; // Nuevo campo para la configuración

        // Corregido: Inyectar IConfiguration
        public ValidateFunction(ILogger<ValidateFunction> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("ValidateToken")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "validate")] HttpRequestData req)
        {
            _logger.LogInformation("HTTP trigger function 'ValidateToken' processed a request.");

            string? token = req.Headers.FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)).Value?.FirstOrDefault();

            if (string.IsNullOrEmpty(token) || !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Authorization token is missing or invalid.");
                return unauthorizedResponse;
            }

            token = token.Substring("Bearer ".Length).Trim();
            
            // Corregido: Obtener la clave secreta de la configuración inyectada
            string? jwtSecret = _configuration.GetValue<string>("JwtSecretKey");
            if (string.IsNullOrEmpty(jwtSecret))
            {
                _logger.LogError("JwtSecretKey is not configured.");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    // Corregido: Establecer los valores de Issuer y Audience
                    ValidateIssuer = true,
                    ValidIssuer = "AuthenticationService",
                    ValidateAudience = true,
                    ValidAudience = "CourseCatalogService",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                SecurityToken validatedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                
                _logger.LogInformation("Token validation successful.");
                
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteStringAsync("Token is valid.");
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed.");
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("Token is invalid or expired.");
                return unauthorizedResponse;
            }
        }
    }
}