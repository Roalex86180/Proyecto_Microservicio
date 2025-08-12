using System.Text.Json.Serialization;

namespace CartService.Models
{
    public class CartItemRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string UserId { get; set; } 
    }
}