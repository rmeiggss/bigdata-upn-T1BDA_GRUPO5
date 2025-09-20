using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace DataAccess.SqlServer.Models
{
    public class CustomerOrders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public required string CustomerId { get; set; }
        public required string CompanyName { get; set; }
        public required string ContactName { get; set; }
        public int TotalOrders { get; set; }
        public required string LastOrderDate { get; set; }
        public required string Country { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
