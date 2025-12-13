using InvoicingCore.Enums;

namespace InvoicingCore.Pdf;

public enum InvoicePdfStyle
{
    Minimal,
    Business
}

public class InvoicePdfModel
{
    //Invoice
    public string InvoiceId { get; set; } = default!;
    public int InvoiceNumber { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string Currency { get; set; } = "CAD";
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Notes { get; set; }

    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public List<InvoicePdfLineItem> LineItems { get; set; } = new();

    //Client
    public string ClientName { get; set; } = default!;
    public string? ClientLegalName { get; set; }
    public string? ClientTaxNumber { get; set; }
    public string? ClientAddress { get; set; }
    public string? ClientContactName { get; set; }
    public string? ClientContactRole { get; set; }
    public string? ClientEmail { get; set; }
    public string? ClientPhone { get; set; }

    //Business profile
    public string BusinessName { get; set; } = string.Empty;
    public string? BusinessLegalName { get; set; }
    public string? BusinessTaxNumber { get; set; }
    public string? BusinessAddress { get; set; }
    public string? BusinessContactName { get; set; }
    public string? BusinessContactRole { get; set; }
    public string? BusinessEmail { get; set; }
    public string? BusinessPhone { get; set; }
    public string? BusinessWebsite { get; set; }

    public string? PaymentInstructions { get; set; }
    public string? DefaultInvoiceNotes { get; set; }
}


public class InvoicePdfLineItem
{
    public string Description { get; set; } = default!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
}
