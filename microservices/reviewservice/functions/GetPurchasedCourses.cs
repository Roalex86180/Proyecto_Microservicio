using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using ReviewService.Models;
using MongoDB.Driver;

namespace ReviewService.Functions
{
    public class GetPurchasedCourses
    {
        private readonly ILogger<GetPurchasedCourses> _logger;
        private readonly CosmosClient _cosmosClient; // Para la base de datos de reseñas (SQL API)
        private readonly IMongoDatabase _mongoDatabase; // Para la base de datos de cursos (MongoDB API)

        public GetPurchasedCourses(ILogger<GetPurchasedCourses> logger, CosmosClient cosmosClient, IMongoDatabase mongoDatabase)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _mongoDatabase = mongoDatabase;
        }

        [Function("GetPurchasedCourses")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{userId}/courses")] HttpRequestData req,
            string userId)
        {
            _logger.LogInformation($"Fetching purchased courses for user ID: {userId}");

            if (string.IsNullOrEmpty(userId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("User ID is required.");
                return badRequestResponse;
            }

            try
            {
                // 1. Obtener IDs de los cursos comprados desde la base de datos de reseñas (SQL API)
                var reviewsDb = _cosmosClient.GetDatabase("AuthDb");
                var purchasesContainer = reviewsDb.GetContainer("Users");

                var query = purchasesContainer.GetItemQueryIterator<PurchasedCourse>(
                    new QueryDefinition("SELECT * FROM c WHERE c.userId = @userId")
                    .WithParameter("@userId", userId));

                var purchasedCourseIds = new List<string>();
                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    purchasedCourseIds.AddRange(response.Select(p => p.CourseId));
                }

                if (!purchasedCourseIds.Any())
                {
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.OK);
                    await notFoundResponse.WriteAsJsonAsync(new List<Course>());
                    return notFoundResponse;
                }
                
                // 2. Obtener los detalles de los cursos desde la base de datos de cursos (MongoDB API)
                var coursesCollection = _mongoDatabase.GetCollection<Course>("Courses");
                var filter = Builders<Course>.Filter.In(c => c.Id, purchasedCourseIds);
                var purchasedCoursesDetails = await coursesCollection.Find(filter).ToListAsync();
                
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(purchasedCoursesDetails);
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchased courses.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Internal server error.");
                return errorResponse;
            }
        }
    }
}