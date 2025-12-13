using InvoicingCore.Enums;
using InvoicingCore.Models;

public class InvoiceResponse
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public int InvoiceNumber { get; set; }

    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string Currency { get; set; } = "CAD";

    public InvoiceStatus Status { get; set; }
    public InvoiceType Type { get; set; }

    public string? Notes { get; set; }

    public List<InvoiceLineItem> LineItems { get; set; } = new();
    public InvoiceTotals Totals { get; set; } = new();

    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public DateTime? SentAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
}
