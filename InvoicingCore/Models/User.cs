namespace InvoicingCore.Models
{
    public class User
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? DisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool EmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public List<UserIdentity> Identities { get; set; } = new();
        public LocalAuthInfo? LocalAuth { get; set; }
        public UserSettings Settings { get; set; } = new();
    }

    public class UserIdentity
    {
        public string Provider { get; set; } = default!;  //google, apple, local
        public string Subject { get; set; } = default!;   //sub claim from token
        public string? Email { get; set; }
        public DateTime LastUsedAt { get; set; }
    }

    public class UserSettings
    {
        public string DefaultCurrency { get; set; } = "CAD";
        public decimal DefaultTaxRate { get; set; } = 0.13m;
    }

    public class LocalAuthInfo
    {
        public string PasswordHash { get; set; } = default!;
        public string Algorithm { get; set; } = "bcrypt";
        public int WorkFactor { get; set; } = 12;
    }

}
