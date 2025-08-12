using System;
using Newtonsoft.Json;

namespace ReviewService.Models
{
    public class Review
    {
        [JsonProperty("id")]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("userId")]
        public string? UserId { get; set; }

        [JsonProperty("courseId")]
        public string? CourseId { get; set; }

        [JsonProperty("rating")]
        public int? Rating { get; set; } // <-- ¡IMPORTANTE! Cambiado a 'int?'

        [JsonProperty("videoUrl")]
        public string? VideoUrl { get; set; }
    }
}