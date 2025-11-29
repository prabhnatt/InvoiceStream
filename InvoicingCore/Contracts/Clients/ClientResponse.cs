using InvoicingCore.Models;

namespace InvoicingCore.Contracts.Clients
{
    public class ClientResponse
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Address? Address { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
