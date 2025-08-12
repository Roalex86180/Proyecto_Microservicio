namespace ReviewService.Models
{
    public class ReviewRequest
    {
        public string? UserId { get; set; }
        public string? CourseId { get; set; }
        public int? Rating { get; set; }
    }
}