// src/PaymentService/Models/UserPurchase.cs

using System;
using System.Text.Json.Serialization;

namespace PaymentService.Models
{
    public class UserPurchase
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        [JsonPropertyName("productId")]
        public string ProductId { get; set; }
        
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }
        
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }
    }
}