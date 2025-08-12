using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver; // Añadido para el driver de MongoDB
using CourseCatalogService.Models;
using Microsoft.Extensions.Configuration; // Añadido

namespace CourseCatalogService.Functions
{
    public class GetCourseById
    {
        private readonly ILogger<GetCourseById> _logger;
        private readonly IMongoDatabase _database; // Inyectamos la interfaz IMongoDatabase
        private readonly IConfiguration _config;

        public GetCourseById(ILogger<GetCourseById> logger, IMongoDatabase database, IConfiguration config)
        {
            _logger = logger;
            _database = database;
            _config = config;
        }

        [Function("GetCourseById")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation($"Getting course with ID: {id}");

            try
            {
                var collectionName = _config["CosmosDb:CollectionName"];
                if (string.IsNullOrEmpty(collectionName))
                {
                    throw new InvalidOperationException("CosmosDb:CollectionName variable not set.");
                }

                var collection = _database.GetCollection<Course>(collectionName);
                
                // Usamos la sintaxis del driver de MongoDB para la búsqueda
                var filter = Builders<Course>.Filter.Eq(c => c.Id, id);
                var course = await collection.Find(filter).FirstOrDefaultAsync();

                if (course == null)
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync($"Course with ID '{id}' not found.");
                    return notFoundResponse;
                }
                
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(course);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course by ID.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error.");
                return errorResponse;
            }
        }
    }
}