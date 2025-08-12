using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CourseCatalogService.Services;

namespace CourseCatalogService.Functions
{
    public class GetCourses
    {
        private readonly ILogger<GetCourses> _logger;
        private readonly CourseService _courseService;

        public GetCourses(ILogger<GetCourses> logger, CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [Function("GetCourses")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "courses")] HttpRequestData req)
        {
            _logger.LogInformation("Getting all courses.");

            var courses = _courseService.Get();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteAsJsonAsync(courses);
            return response;
        }
    }
}