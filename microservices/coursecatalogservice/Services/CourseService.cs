using MongoDB.Driver;
using CourseCatalogService.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration; 

namespace CourseCatalogService.Services
{
    public class CourseService
    {
        private readonly IMongoCollection<Course> _courses;

        public CourseService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetValue<string>("CosmosDb:ConnectionString"));
            var database = client.GetDatabase(configuration.GetValue<string>("CosmosDb:DatabaseName"));
            _courses = database.GetCollection<Course>(configuration.GetValue<string>("CosmosDb:CollectionName"));
        }

        public List<Course> Get() =>
            _courses.Find(course => true).ToList();

        public Course Get(string id) =>
            _courses.Find<Course>(course => course.Id == id).FirstOrDefault(); 

        public Course Create(Course course)
        {
            _courses.InsertOne(course);
            return course;
        }

        public void Update(string id, Course courseIn)
        {
            courseIn.Id = id;
            _courses.ReplaceOne(course => course.Id == id, courseIn);
        }

        public void Remove(string id) =>
            _courses.DeleteOne(course => course.Id == id);
    }
}