using InvoicingCore.Models;
using MongoDB.Driver;

namespace InvoicingCore.Services
{
    public class InvoiceNumberService
    {
        private readonly MongoDbContext _db;

        public InvoiceNumberService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetNextInvoiceNumberAsync(string userId, CancellationToken ct = default)
        {
            var filter = Builders<InvoiceSequence>.Filter.Eq(x => x.Id, userId);

            var update = Builders<InvoiceSequence>.Update
                .Inc(x => x.NextInvoiceNumber, 1)
                .SetOnInsert(x => x.Id, userId); //so first upsert sets Id to userId

            var options = new FindOneAndUpdateOptions<InvoiceSequence>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            };

            var result = await _db.InvoiceSequences
                .FindOneAndUpdateAsync(filter, update, options, ct);

            return result.NextInvoiceNumber;
        }
    }
}
