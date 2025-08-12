using System.Text.Json.Serialization;

namespace AuthenticationService.Models
{
    public class DeleteRequest
    {
        [JsonPropertyName("username")]
        public required string Username { get; set; }
    }
}