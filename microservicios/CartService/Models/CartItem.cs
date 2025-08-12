using System.Text.Json.Serialization;

namespace CartService.Models
{
    public class CartItem
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        // Esta es la propiedad clave de partición. Es obligatoria para guardar.
        [JsonPropertyName("userId")]
        public required string UserId { get; set; }

        public required string ProductId { get; set; }

        public required string ProductName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}