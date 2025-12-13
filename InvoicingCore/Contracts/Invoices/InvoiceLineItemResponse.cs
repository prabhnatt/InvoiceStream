namespace InvoicingCore.Contracts.Invoices
{
    public class InvoiceLineItemResponse
    {
        public string Description { get; set; } = default!;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TaxRate { get; set; }
        public decimal LineTotal { get; set; }
    }
}
