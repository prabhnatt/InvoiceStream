using InvoicingCore.Models;

namespace InvoicingCore.Contracts.Invoices
{
    public class InvoiceResponse
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public int InvoiceNumber { get; set; }
        public string Type { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public string Currency { get; set; } = default!;
        public List<InvoiceLineItem> LineItems { get; set; } = new();
        public InvoiceTotals Totals { get; set; } = new();
        public string? Notes { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
