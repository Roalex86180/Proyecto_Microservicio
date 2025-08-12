using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CourseCatalogService.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("platform")]
        public required string Platform { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("price")]
        public decimal Price { get; set; }

        [BsonElement("image")]
        public required string Image { get; set; }
    }
}
