using MongoDB.Bson;

namespace DataAccess.MongoDb.Models
{
    public class MongoDBbase
    {
        public ObjectId Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    }
}
