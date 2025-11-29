using System;
using System.Collections.Generic;
using System.Text;

namespace InvoicingCore.Models
{
    public class VerificationCode
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;

        /// <summary>
        /// Purpose of the code: "email_verification", "login", "password_reset"
        /// </summary>
        public string Purpose { get; set; } = default!;
        
        public string CodeHash { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
