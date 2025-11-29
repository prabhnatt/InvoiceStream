namespace InvoicingCore.Models
{
    public class Invoice
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public int InvoiceNumber { get; set; }
        public string Type { get; set; } = "Invoice";
        public string Status { get; set; } = "Draft";
        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }
        public string Currency { get; set; } = "CAD";
        public List<InvoiceLineItem> LineItems { get; set; } = new();
        public InvoiceTotals Totals { get; set; } = new();
        public string? Notes { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
