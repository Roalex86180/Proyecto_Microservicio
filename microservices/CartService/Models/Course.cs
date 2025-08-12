namespace CartService.Models
{
    public class Course
    {
        public string Id { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Image { get; set; } = string.Empty;
    }
}