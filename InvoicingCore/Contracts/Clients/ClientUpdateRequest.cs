using InvoicingCore.Models;

namespace InvoicingCore.Contracts.Clients
{
    public class ClientUpdateRequest
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = string.Empty;
        public string? LegalName { get; set; }
        public string? TaxNumber { get; set; }
        public Address? Address { get; set; }
        public string? PrimaryContactName { get; set; }
        public string? PrimaryContactRole { get; set; }
        public string? PrimaryEmail { get; set; }
        public string? PrimaryPhone { get; set; }
        public string? DefaultCurrency { get; set; }
        public int? DefaultPaymentTermsDays { get; set; }
        public decimal? DefaultTaxRate { get; set; }
        public string? Notes { get; set; }
    }

}
