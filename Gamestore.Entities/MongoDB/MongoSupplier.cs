#nullable disable
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Gamestore.Entities.MongoDB;

public class MongoSupplier
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("SupplierID")]
    public int SupplierId { get; set; }

    [BsonElement("CompanyName")]
    public string CompanyName { get; set; }

    [BsonElement("ContactName")]
    public string ContactName { get; set; }

    [BsonElement("ContactTitle")]
    public string ContactTitle { get; set; }

    [BsonElement("Address")]
    public string Address { get; set; }

    [BsonElement("City")]
    public string City { get; set; }

    [BsonElement("Region")]
    public string Region { get; set; }

    [BsonElement("PostalCode")]
    public string PostalCode { get; set; }

    [BsonElement("Country")]
    public string Country { get; set; }

    [BsonElement("Phone")]
    public string Phone { get; set; }

    [BsonElement("Fax")]
    public string Fax { get; set; }

    [BsonElement("HomePage")]
    public string HomePage { get; set; }
}