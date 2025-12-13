namespace InvoicingCore.Models;

public class BusinessProfile
{
    public string Id { get; set; } = default!;
    public string UserId { get; set; } = default!;

    public string BusinessName { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxNumber { get; set; }  //HST/GST

    public Address? Address { get; set; }

    public string? LogoUrl { get; set; }

    public string? PrimaryContactName { get; set; }
    public string? PrimaryContactRole { get; set; }
    public string? PrimaryEmail { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    public string DefaultCurrency { get; set; } = "CAD";
    public decimal DefaultTaxRate { get; set; } = 0.13m;
    public int DefaultPaymentTermsDays { get; set; } = 14;

    public string? DefaultInvoiceNotes { get; set; }
    public string? PaymentInstructions { get; set; }
}
