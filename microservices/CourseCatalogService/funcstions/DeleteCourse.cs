using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CourseCatalogService.Services;

namespace CourseCatalogService.Functions
{
    public class DeleteCourse
    {
        private readonly ILogger<DeleteCourse> _logger;
        private readonly CourseService _courseService;

        public DeleteCourse(ILogger<DeleteCourse> logger, CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [Function("DeleteCourse")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "courses/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation($"Deleting course with ID: {id}");

            _courseService.Remove(id);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("Course deleted successfully.");
            return response;
        }
    }
}