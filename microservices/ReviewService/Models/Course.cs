using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ReviewService.Models
{
    [BsonIgnoreExtraElements] // Ignora campos adicionales no mapeados
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string? Name { get; set; }

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("platform")]
        public string? Platform { get; set; }

        // AGREGAR ESTA PROPIEDAD para solucionar el error de deserialización
        [BsonElement("image")]
        public string? Image { get; set; }
    }
}