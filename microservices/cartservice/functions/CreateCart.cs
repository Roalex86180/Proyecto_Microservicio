using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.IO;

namespace CartService.Functions
{
    public class CreateCart
    {
        private readonly ILogger<CreateCart> _logger;

        public CreateCart(ILogger<CreateCart> logger)
        {
            _logger = logger;
        }

        [Function("CreateCart")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cart/create")] HttpRequestData req)
        {
            _logger.LogInformation("Create cart request received.");

            string requestBody;
            try
            {
                // Lee el cuerpo de la solicitud para obtener el userId
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("Request body is empty.");
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Request body is empty.");
                    return badRequestResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading request body.");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteStringAsync("Invalid request body.");
                return errorResponse;
            }

            // Simplemente devuelve una respuesta de éxito
            // La lógica de añadir el primer ítem se manejará en la función AddToCart
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync("Cart endpoint is working. Ready to add items.");
            return response;
        }
    }
}