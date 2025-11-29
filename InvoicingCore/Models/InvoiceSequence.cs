using MongoDB.Bson.Serialization.Attributes;

namespace InvoicingCore.Models
{
    public class InvoiceSequence
    {
        [BsonId]
        public string Id { get; set; } = default!;
        public int NextInvoiceNumber { get; set; }
    }
}
