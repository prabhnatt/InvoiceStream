namespace InvoicingCore.Contracts.Invoices
{
    public class InvoiceCreateRequest
    {
        public string ClientId { get; set; } = default!;

        public DateOnly IssueDate { get; set; }
        public DateOnly DueDate { get; set; }

        public string Currency { get; set; } = "CAD";

        public List<InvoiceLineItemRequest> LineItems { get; set; } = new();

        public string? Notes { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
