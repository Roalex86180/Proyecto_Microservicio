using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Linq;
using ReviewService.Models;

namespace ReviewService.Functions
{
    public class GetReviews
    {
        private readonly ILogger<GetReviews> _logger;
        private readonly CosmosClient _cosmosClient;

        public GetReviews(ILogger<GetReviews> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        [Function("GetReviews")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "reviews/{courseId}")] HttpRequestData req,
            string courseId)
        {
            _logger.LogInformation($"Getting reviews for courseId: {courseId}");

            if (string.IsNullOrEmpty(courseId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("CourseId is required to get reviews.");
                return badRequestResponse;
            }

            try
            {
                var database = _cosmosClient.GetDatabase("ReviewsDb");
                var container = database.GetContainer("Reviews");

                var query = new QueryDefinition("SELECT * FROM c WHERE c.courseId = @courseId");
                query.WithParameter("@courseId", courseId);

                var reviews = new List<Review>();
                using (FeedIterator<Review> feedIterator = container.GetItemQueryIterator<Review>(query))
                {
                    while (feedIterator.HasMoreResults)
                    {
                        foreach (var item in await feedIterator.ReadNextAsync())
                        {
                            reviews.Add(item);
                        }
                    }
                }

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteAsJsonAsync(reviews);
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reviews from Cosmos DB.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error retrieving reviews.");
                return errorResponse;
            }
        }
    }
}