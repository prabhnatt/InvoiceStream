namespace InvoicingServer.Models
{
    public class LocalRegisterRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string? DisplayName { get; set; }
    }

    public class LocalLoginRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
