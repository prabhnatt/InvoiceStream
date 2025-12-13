using InvoicingCore;
using InvoicingCore.Pdf;
using MongoDB.Driver;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace InvoicingAPI.Services;

public class InvoicePdfService
{
    private readonly MongoDbContext _db;

    public InvoicePdfService(MongoDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(
        string invoiceId,
        InvoicePdfStyle style,
        string userId,
        CancellationToken ct = default)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var invoice = await _db.Invoices
            .Find(x => x.Id == invoiceId && x.UserId == userId)
            .FirstOrDefaultAsync(ct);

        if (invoice is null)
            throw new InvalidOperationException("Invoice not found.");

        var client = await _db.Clients
            .Find(c => c.Id == invoice.ClientId && c.UserId == userId)
            .FirstOrDefaultAsync(ct);

        var profile = await _db.BusinessProfiles
            .Find(p => p.UserId == userId)
            .FirstOrDefaultAsync(ct);

        var model = new InvoicePdfModel
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Status = invoice.Status,
            Currency = invoice.Currency,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            Notes = invoice.Notes,
            SubTotal = invoice.Totals.SubTotal,
            Tax = invoice.Totals.Tax,
            Total = invoice.Totals.GrandTotal,

            ClientName = client?.Name ?? "(Unknown client)",
            ClientLegalName = client?.LegalName,
            ClientTaxNumber = client?.TaxNumber,
            ClientAddress = client?.Address?.ToString(),
            ClientContactName = client?.PrimaryContactName,
            ClientContactRole = client?.PrimaryContactRole,
            ClientEmail = client?.PrimaryEmail,
            ClientPhone = client?.PrimaryPhone,

            BusinessName = profile?.BusinessName ?? "Invoice Stream Client",
            BusinessLegalName = profile?.LegalName,
            BusinessTaxNumber = profile?.TaxNumber,
            BusinessAddress = profile?.Address?.ToString(),
            BusinessContactName = profile?.PrimaryContactName,
            BusinessContactRole = profile?.PrimaryContactRole,
            BusinessEmail = profile?.PrimaryEmail,
            BusinessPhone = profile?.PrimaryPhone,
            BusinessWebsite = profile?.Website,
            PaymentInstructions = profile?.PaymentInstructions,
            DefaultInvoiceNotes = profile?.DefaultInvoiceNotes
        };

        foreach (var li in invoice.LineItems)
        {
            var baseAmount = li.Quantity * li.UnitPrice;
            var tax = baseAmount * li.TaxRate;
            model.LineItems.Add(new InvoicePdfLineItem
            {
                Description = li.Description,
                Quantity = li.Quantity,
                UnitPrice = li.UnitPrice,
                TaxRate = li.TaxRate,
                LineTotal = baseAmount + tax
            });
        }

        var doc = new InvoicePdfDocument(model, style);
        return doc.GeneratePdf();
    }
}