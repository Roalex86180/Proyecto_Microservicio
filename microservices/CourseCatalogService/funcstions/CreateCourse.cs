using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CourseCatalogService.Models;
using CourseCatalogService.Services;

namespace CourseCatalogService.Functions
{
    public class CreateCourse
    {
        private readonly ILogger<CreateCourse> _logger;
        private readonly CourseService _courseService;

        public CreateCourse(ILogger<CreateCourse> logger, CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [Function("CreateCourse")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "courses")] HttpRequestData req)
        {
            _logger.LogInformation("Creating a new course.");

            var course = await req.ReadFromJsonAsync<Course>();
            if (course == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                badRequest.WriteString("Invalid course data provided.");
                return badRequest;
            }

            _courseService.Create(course);
            
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(course);
            return response;
        }
    }
}