// src/PaymentService/Models/PaymentRequest.cs (Ejemplo)

using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class PaymentRequest
    {
        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("courseId")]
        public string? CourseId { get; set; }
        
        // [NUEVO] Propiedades necesarias para pagos directos
        [JsonPropertyName("productName")]
        public string? ProductName { get; set; }
        
        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }
}