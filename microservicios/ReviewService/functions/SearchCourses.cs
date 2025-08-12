using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Bson; // ← AGREGAR ESTA LÍNEA
using ReviewService.Models;
using System.Web; // ← AGREGAR ESTA LÍNEA para HttpUtility

namespace ReviewService.Functions
{
    public class SearchCourses
    {
        private readonly ILogger<SearchCourses> _logger;
        private readonly IMongoClient _mongoClient;
        private const string DatabaseName = "aca-db";
        private const string CollectionName = "courses";

        public SearchCourses(ILogger<SearchCourses> logger, IMongoClient mongoClient)
        {
            _logger = logger;
            _mongoClient = mongoClient;
        }

        [Function("SearchCourses")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses/search")] HttpRequestData req)
        {
            _logger.LogInformation("Course search request received.");

            // Corregir la obtención de query parameters para Azure Functions v4
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var queryName = query["name"];

            if (string.IsNullOrEmpty(queryName))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("A search 'name' query parameter is required.");
                return badRequestResponse;
            }

            try
            {
                var database = _mongoClient.GetDatabase(DatabaseName);
                var collection = database.GetCollection<Course>(CollectionName);

                // Construir un filtro de búsqueda por nombre, sin distinción entre mayúsculas y minúsculas
                var filter = Builders<Course>.Filter.Regex("name", new BsonRegularExpression(queryName, "i"));
                
                var courses = await collection.Find(filter).ToListAsync();
                
                _logger.LogInformation($"Found {courses.Count} courses for search term '{queryName}'.");

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(courses);
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for courses in MongoDB.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error searching for courses.");
                return errorResponse;
            }
        }
    }
}