using InvoicingCore.Enums;

public class MarkPaidRequest
{
    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
}
