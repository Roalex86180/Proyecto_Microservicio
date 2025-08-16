// src/PaymentService/Models/PaymentRequest.cs

namespace PaymentService.Models
{
    public class PaymentRequest
    {
        public string? UserId { get; set; }
        // [NUEVO] Agrega esta propiedad para recibir el ID del curso
        public string? CourseId { get; set; } 
    }
}