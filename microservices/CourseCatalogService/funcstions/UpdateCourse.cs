using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CourseCatalogService.Models;
using CourseCatalogService.Services;

namespace CourseCatalogService.Functions
{
    public class UpdateCourse
    {
        private readonly ILogger<UpdateCourse> _logger;
        private readonly CourseService _courseService;

        public UpdateCourse(ILogger<UpdateCourse> logger, CourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        [Function("UpdateCourse")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "courses/{id}")] HttpRequestData req,
            string id)
        {
            _logger.LogInformation($"Updating course with ID: {id}");

            var course = await req.ReadFromJsonAsync<Course>();
            if (course == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid course data provided.");
                return badRequest;
            }

            _courseService.Update(id, course);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync("Course updated successfully."); // <<< CAMBIO AQUÍ
            return response;
        }
    }
}