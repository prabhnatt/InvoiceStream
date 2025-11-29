using InvoicingCore.Configuration;
using InvoicingCore.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace InvoicingCore
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoSettings> options)
        {
            var settings = options.Value;

            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new InvalidOperationException("Mongo ConnectionString is not configured.");

            if (string.IsNullOrWhiteSpace(settings.DatabaseName))
                throw new InvalidOperationException("Mongo DatabaseName is not configured.");

            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Client> Clients => _database.GetCollection<Client>("clients");
        public IMongoCollection<Invoice> Invoices => _database.GetCollection<Invoice>("invoices");
        public IMongoCollection<InvoiceSequence> InvoiceSequences => _database.GetCollection<InvoiceSequence>("invoiceSequences");
        public IMongoCollection<VerificationCode> VerificationCodes => _database.GetCollection<VerificationCode>("verificationCodes");
    }

}
