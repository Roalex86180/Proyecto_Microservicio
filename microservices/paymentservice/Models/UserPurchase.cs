// src/PaymentService/Models/UserPurchase.cs

using System;
using Newtonsoft.Json;

namespace PaymentService.Models
{
    public class UserPurchase
    {
        // [CORRECCIÓN] Agregamos el modificador 'required'
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("userId")]
        public required string UserId { get; set; }

        [JsonProperty("productId")]
        public required string ProductId { get; set; }

        [JsonProperty("productName")]
        public required string ProductName { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("purchaseDate")]
        public DateTime PurchaseDate { get; set; }
    }
}