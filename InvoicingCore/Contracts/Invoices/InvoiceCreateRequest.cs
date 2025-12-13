using InvoicingCore.Enums;
using InvoicingCore.Models;

namespace InvoicingCore.Contracts.Invoices
{
    public class InvoiceCreateRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public string Currency { get; set; } = "CAD";
        public InvoiceType Type { get; set; } = InvoiceType.Invoice;
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
        public List<InvoiceLineItem> LineItems { get; set; } = new();
        public InvoiceTotals Totals { get; set; } = new();
        public string? Notes { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
