namespace InvoicingCore.Models
{
    public class Client
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Address? Address { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
