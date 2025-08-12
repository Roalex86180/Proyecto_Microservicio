using Newtonsoft.Json;

namespace AuthenticationService.Models
{
    public class User
    {
        // El 'id' de Cosmos DB es el identificador único del documento.
        // Usamos una propiedad para que se genere un nuevo GUID si no se especifica uno.
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // El nombre de usuario. Es una buena práctica usarlo como clave de partición.
        // Lo marcamos como obligatorio.
        [JsonProperty("username")]
        public string Username { get; set; } = string.Empty;

        // El hash de la contraseña. Nunca almacenes la contraseña en texto plano.
        [JsonProperty("passwordHash")]
        public string PasswordHash { get; set; } = string.Empty;

        // El correo electrónico del usuario.
        [JsonProperty("email")]
        public string Email { get; set; } = string.Empty;
    }
}