using System.Text.Json.Serialization;

namespace AuthenticationService.Models
{
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public required string Username { get; set; }

        [JsonPropertyName("password")]
        public required string Password { get; set; }
    }
}