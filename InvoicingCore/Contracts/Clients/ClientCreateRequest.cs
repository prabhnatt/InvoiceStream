using InvoicingCore.Models;

namespace InvoicingCore.Contracts.Clients
{
    public class ClientCreateRequest
    {
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Address? Address { get; set; }
        public string? Notes { get; set; }
    }
}
