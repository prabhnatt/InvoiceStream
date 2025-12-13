using InvoicingCore.Contracts.Invoices;
using InvoicingCore.Models;
using MongoDB.Driver;

namespace InvoicingCore.Services
{
    public class InvoiceService
    {
        private readonly MongoDbContext _db;
        private readonly InvoiceNumberService _invoiceNumberService;

        public InvoiceService(MongoDbContext db, InvoiceNumberService invoiceNumberService)
        {
            _db = db;
            _invoiceNumberService = invoiceNumberService;
        }

        public async Task<Invoice> CreateInvoiceAsync(string userId, InvoiceCreateRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId))
                throw new ArgumentException("ClientId is required.", nameof(request.ClientId));

            if (request.LineItems is null || request.LineItems.Count == 0)
                throw new ArgumentException("At least one line item is required.", nameof(request.LineItems));

            //Get the next invoice number for this user
            var invoiceNumber = await _invoiceNumberService.GetNextInvoiceNumberAsync(userId, ct);

            var now = DateTime.UtcNow;

            var lineItems = request.LineItems.Select(li => new InvoiceLineItem
            {
                Description = li.Description.Trim(),
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TaxRate = li.TaxRate
            }).ToList();

            var totals = CalculateTotals(lineItems);

            var invoice = new Invoice
            {
                Id = Guid.NewGuid().ToString("N"),
                UserId = userId,
                ClientId = request.ClientId,
                InvoiceNumber = invoiceNumber,
                Type = request.Type,
                Status = request.Status,
                IssueDate = request.IssueDate,
                DueDate = request.DueDate,
                Currency = request.Currency,
                LineItems = lineItems,
                Totals = totals,
                Notes = request.Notes,
                Tags = request.Tags ?? new List<string>(),
                CreatedAt = now,
                UpdatedAt = now
            };

            await _db.Invoices.InsertOneAsync(invoice, cancellationToken: ct);

            return invoice;
        }

        public async Task<List<Invoice>> GetInvoicesForUserAsync(string userId, CancellationToken ct = default)
        {
            var filter = Builders<Invoice>.Filter.Eq(i => i.UserId, userId);

            return await _db.Invoices
                .Find(filter)
                .SortByDescending(i => i.IssueDate)
                .ToListAsync(ct);
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(string userId, string invoiceId, CancellationToken ct = default)
        {
            var filter = Builders<Invoice>.Filter.And(
                Builders<Invoice>.Filter.Eq(i => i.UserId, userId),
                Builders<Invoice>.Filter.Eq(i => i.Id, invoiceId)
            );

            return await _db.Invoices.Find(filter).FirstOrDefaultAsync(ct);
        }

        private static InvoiceTotals CalculateTotals(List<InvoiceLineItem> items)
        {
            decimal subTotal = 0;
            decimal tax = 0;

            foreach (var item in items)
            {
                var lineBase = item.Quantity * item.UnitPrice;
                subTotal += lineBase;
                tax += lineBase * item.TaxRate;
            }

            return new InvoiceTotals
            {
                SubTotal = decimal.Round(subTotal, 2),
                Tax = decimal.Round(tax, 2),
                GrandTotal = decimal.Round(subTotal + tax, 2)
            };
        }

        public async Task<Invoice?> UpdateInvoiceAsync(string userId, string invoiceId, InvoiceCreateRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.ClientId))
                throw new ArgumentException("ClientId is required.", nameof(request.ClientId));

            if (request.LineItems is null || request.LineItems.Count == 0)
                throw new ArgumentException("At least one line item is required.", nameof(request.LineItems));

            var lineItems = request.LineItems.Select(li => new InvoiceLineItem
            {
                Description = li.Description.Trim(),
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TaxRate = li.TaxRate
            }).ToList();

            var totals = CalculateTotals(lineItems);

            var filter = Builders<Invoice>.Filter.And(
                Builders<Invoice>.Filter.Eq(i => i.UserId, userId),
                Builders<Invoice>.Filter.Eq(i => i.Id, invoiceId)
            );

            var update = Builders<Invoice>.Update
                .Set(i => i.ClientId, request.ClientId)
                .Set(i => i.IssueDate, request.IssueDate)
                .Set(i => i.DueDate, request.DueDate)
                .Set(i => i.Currency, request.Currency)
                .Set(i => i.Type, request.Type)
                .Set(i => i.Status, request.Status)
                .Set(i => i.LineItems, lineItems)
                .Set(i => i.Totals, totals)
                .Set(i => i.Notes, request.Notes)
                .Set(i => i.Tags, request.Tags ?? new List<string>())
                .Set(i => i.UpdatedAt, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<Invoice>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _db.Invoices.FindOneAndUpdateAsync(filter, update, options, ct);
        }

        public async Task<bool> DeleteInvoiceAsync(string userId, string invoiceId, CancellationToken ct = default)
        {
            var filter = Builders<Invoice>.Filter.And(
                Builders<Invoice>.Filter.Eq(i => i.UserId, userId),
                Builders<Invoice>.Filter.Eq(i => i.Id, invoiceId)
            );

            var result = await _db.Invoices.DeleteOneAsync(filter, ct);
            return result.DeletedCount > 0;
        }

        public static InvoiceResponse ToResponse(Invoice invoice)
        {
            return new()
            {
                Id = invoice.Id,
                UserId = invoice.UserId,
                ClientId = invoice.ClientId,
                InvoiceNumber = invoice.InvoiceNumber,
                Type = invoice.Type,
                Status = invoice.Status,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Currency = invoice.Currency,
                LineItems = invoice.LineItems,
                Totals = invoice.Totals,
                Notes = invoice.Notes,
                Tags = invoice.Tags,
                CreatedAt = invoice.CreatedAt,
                UpdatedAt = invoice.UpdatedAt
            };
        }
    }
}
