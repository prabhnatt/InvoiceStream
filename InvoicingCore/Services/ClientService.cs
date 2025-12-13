using InvoicingCore.Contracts.Clients;
using InvoicingCore.Models;
using MongoDB.Driver;

namespace InvoicingCore.Services
{
    public class ClientService
    {
        private readonly MongoDbContext _db;

        public ClientService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<Client> CreateClientAsync(string userId, ClientCreateRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Client name is required.", nameof(request.Name));
            }

            var now = DateTime.UtcNow;

            var client = new Client
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                Name = request.Name.Trim(),
                PrimaryEmail = request.PrimaryEmail,
                PrimaryPhone = request.PrimaryPhone,
                Address = request.Address,
                Notes = request.Notes,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.Clients.InsertOneAsync(client, cancellationToken: ct);

            return client;
        }

        public async Task<List<Client>> GetClientsForUserAsync(string userId, CancellationToken ct = default)
        {
            var filter = Builders<Client>.Filter.Eq(c => c.UserId, userId);

            return await _db.Clients
                .Find(filter)
                .SortBy(c => c.Name)
                .ToListAsync(ct);
        }

        public async Task<Client?> GetClientByIdAsync(string userId, string clientId, CancellationToken ct = default)
        {
            var filter = Builders<Client>.Filter.And(
                Builders<Client>.Filter.Eq(c => c.UserId, userId),
                Builders<Client>.Filter.Eq(c => c.Id, clientId)
            );

            return await _db.Clients.Find(filter).FirstOrDefaultAsync(ct);
        }

        public async Task<Client?> UpdateClientAsync(string userId, string clientId, ClientCreateRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Client name is required.", nameof(request.Name));
            }

            var filter = Builders<Client>.Filter.And(
                Builders<Client>.Filter.Eq(c => c.UserId, userId),
                Builders<Client>.Filter.Eq(c => c.Id, clientId)
            );

            var update = Builders<Client>.Update
                .Set(c => c.Name, request.Name.Trim())
                .Set(c => c.PrimaryEmail, request.PrimaryEmail)
                .Set(c => c.PrimaryPhone, request.PrimaryPhone)
                .Set(c => c.Address, request.Address)
                .Set(c => c.Notes, request.Notes)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<Client>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _db.Clients.FindOneAndUpdateAsync(filter, update, options, ct);
        }

        public async Task<bool> DeleteClientAsync(string userId, string clientId, CancellationToken ct = default)
        {
            var filter = Builders<Client>.Filter.And(
                Builders<Client>.Filter.Eq(c => c.UserId, userId),
                Builders<Client>.Filter.Eq(c => c.Id, clientId)
            );

            var result = await _db.Clients.DeleteOneAsync(filter, ct);
            return result.DeletedCount > 0;
        }

        public static ClientResponse ToResponse(Client c)
        {
            return new()
            {
                Id = c.Id,
                Name = c.Name,
                PrimaryEmail = c.PrimaryEmail,
                PrimaryPhone = c.PrimaryPhone,
                Address = c.Address,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }

}
