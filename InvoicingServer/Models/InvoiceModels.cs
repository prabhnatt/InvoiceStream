using InvoicingCore.Models;

namespace InvoicingServer.Models;

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

public class InvoiceCreateRequest
{
    public string ClientId { get; set; } = string.Empty;
    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(14));
    public string Currency { get; set; } = "CAD";
    public List<InvoiceLineItem> LineItems { get; set; } = new()
    {
        new InvoiceLineItem
        {
            Description = "",
            Quantity = 1,
            UnitPrice = 0,
            TaxRate = 0.13m
        }
    };
    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = new();
}
