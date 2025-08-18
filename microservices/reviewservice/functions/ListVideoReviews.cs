using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ReviewService.Functions
{
    public class ListVideoReviews
    {
        private readonly ILogger<ListVideoReviews> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public ListVideoReviews(ILogger<ListVideoReviews> logger, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        [Function("ListVideoReviews")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "video-reviews")] HttpRequestData req)
        {
            _logger.LogInformation("Request received to list all video reviews.");

            string containerName = _configuration["AzureBlobStorageContainerName"] ?? "videoreviews";

            try
            {
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                if (!await blobContainerClient.ExistsAsync())
                {
                    _logger.LogWarning($"Container '{containerName}' does not exist.");
                    var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                    await notFoundResponse.WriteStringAsync("Video reviews container not found.");
                    return notFoundResponse;
                }

                var videoReviews = new List<object>();

                await foreach (var blobItem in blobContainerClient.GetBlobsAsync())
                {
                    var blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                    videoReviews.Add(new 
                    {
                        Name = blobItem.Name,
                        Url = blobClient.Uri.ToString()
                    });
                }

                _logger.LogInformation($"Found {videoReviews.Count} video reviews.");
                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(videoReviews);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing video reviews.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error retrieving video list.");
                return errorResponse;
            }
        }
    }
}