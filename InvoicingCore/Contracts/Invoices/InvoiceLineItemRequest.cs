namespace InvoicingCore.Contracts.Invoices
{
    public class InvoiceLineItemRequest
    {
        public string Description { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
    }
}
