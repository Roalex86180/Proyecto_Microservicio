// src/ReviewService/Models/PurchasedCourse.cs
using System;
using Newtonsoft.Json;

namespace ReviewService.Models
{
    public class PurchasedCourse
    {
        [JsonProperty("id")]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("userId")]
        public string? UserId { get; set; }
        
        [JsonProperty("courseId")]
        public string? CourseId { get; set; }

        [JsonProperty("purchaseDate")]
        public DateTime PurchaseDate { get; set; }
    }
}