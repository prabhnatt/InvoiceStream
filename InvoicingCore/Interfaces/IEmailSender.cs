using System;
using System.Collections.Generic;
using System.Text;

namespace InvoicingCore.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    }
}
