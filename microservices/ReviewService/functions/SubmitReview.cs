using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using System.IO;
using System.Linq;
using ReviewService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;

namespace ReviewService.Functions
{
    public class SubmitReview
    {
        private readonly ILogger<SubmitReview> _logger;
        private readonly CosmosClient _cosmosClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        private const int MaxVideoSizeInBytes = 3 * 60 * 1024 * 1024;

        public SubmitReview(ILogger<SubmitReview> logger, CosmosClient cosmosClient, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        [Function("SubmitReview")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Review submission request received.");

            var contentType = req.Headers.GetValues("Content-Type")?.FirstOrDefault();

            if (string.IsNullOrEmpty(contentType) || !contentType.Contains("multipart/form-data"))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid content type. Expected 'multipart/form-data'.");
                return badRequestResponse;
            }

            var mediaTypeHeader = MediaTypeHeaderValue.Parse(contentType);
            var boundary = mediaTypeHeader.Boundary;
            var boundaryString = boundary.HasValue ? HeaderUtilities.RemoveQuotes(boundary.Value).Value : string.Empty;
            
            if (string.IsNullOrEmpty(boundaryString))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid multipart boundary.");
                return badRequestResponse;
            }
            
            var reader = new MultipartReader(boundaryString, req.Body);
            var section = await reader.ReadNextSectionAsync();

            var reviewRequest = new ReviewRequest();
            Stream? videoStream = null;
            string videoFileName = string.Empty;

            while (section != null)
            {
                var contentDispositionHeader = section.Headers["Content-Disposition"].FirstOrDefault();
                if (!string.IsNullOrEmpty(contentDispositionHeader) && 
                    ContentDispositionHeaderValue.TryParse(contentDispositionHeader, out var contentDisposition))
                {
                    // Corregir el manejo del StringSegment
                    var name = contentDisposition.Name.HasValue ? 
                               contentDisposition.Name.Value.Replace("\"", "") : null;
                    
                    if (name == "video" && contentDisposition.FileName.HasValue && !string.IsNullOrEmpty(contentDisposition.FileName.Value))
                    {
                        videoStream = section.Body;
                        // Corregir la conversión de StringSegment a string
                        videoFileName = contentDisposition.FileName.Value.Replace("\"", "");
                    }
                    else
                    {
                        using var streamReader = new StreamReader(section.Body);
                        var value = await streamReader.ReadToEndAsync();
                        if (name == "userId") reviewRequest.UserId = value;
                        else if (name == "courseId") reviewRequest.CourseId = value;
                        else if (name == "rating" && int.TryParse(value, out int ratingValue)) reviewRequest.Rating = ratingValue;
                    }
                }
                section = await reader.ReadNextSectionAsync();
            }

            if (string.IsNullOrEmpty(reviewRequest.CourseId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("CourseId is required to submit a review.");
                return badRequestResponse;
            }

            bool hasVideo = videoStream != null && videoStream.Length > 0;
            if (hasVideo)
            {
                if (string.IsNullOrEmpty(reviewRequest.UserId))
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("UserId is required when submitting a video review.");
                    return badRequestResponse;
                }

                if (videoStream!.Length > MaxVideoSizeInBytes)
                {
                    var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                    await badRequestResponse.WriteStringAsync("Video size exceeds the maximum limit of 3 minutes.");
                    return badRequestResponse;
                }
            }

            // Validar rating si se proporciona (independientemente de si hay video)
            if (reviewRequest.Rating.HasValue && (reviewRequest.Rating < 1 || reviewRequest.Rating > 5))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Rating must be between 1 and 5.");
                return badRequestResponse;
            }

            // Si no hay video, el rating es obligatorio
            if (!hasVideo && reviewRequest.Rating == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("A rating (1-5) is required if no video is provided.");
                return badRequestResponse;
            }

            string videoUrl = string.Empty;

            if (hasVideo)
            {
                string containerName = _configuration["AzureBlobStorageContainerName"] ?? "videoreviews";
                var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await blobContainerClient.CreateIfNotExistsAsync();

                var blobName = $"{reviewRequest.CourseId}/{reviewRequest.UserId}/{Guid.NewGuid()}{Path.GetExtension(videoFileName)}";
                var blobClient = blobContainerClient.GetBlobClient(blobName);
                
                await blobClient.UploadAsync(videoStream, overwrite: true);

                videoUrl = blobClient.Uri.ToString();
                _logger.LogInformation($"Video uploaded to: {videoUrl}");
            }

            try
            {
                var database = _cosmosClient.GetDatabase("ReviewsDb");
                var container = database.GetContainer("Reviews");

                var review = new Review
                {
                    UserId = reviewRequest.UserId,
                    CourseId = reviewRequest.CourseId,
                    Rating = reviewRequest.Rating,
                    VideoUrl = videoUrl
                };

                await container.CreateItemAsync(review, new PartitionKey(review.CourseId));
                _logger.LogInformation($"Review for course '{review.CourseId}' submitted successfully.");

                var successResponse = req.CreateResponse(HttpStatusCode.Created);
                await successResponse.WriteAsJsonAsync(new { message = "Review submitted successfully.", reviewId = review.Id });
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving review to Cosmos DB.");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error saving review to database.");
                return errorResponse;
            }
        }
    }
}