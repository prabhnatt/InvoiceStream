using InvoicingCore.Interfaces;

namespace InvoicingServer.Services
{
    public class DevEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("==== DEV EMAIL SENDER ====");
            Console.WriteLine($"To: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine("Body:");
            Console.WriteLine(body);
            Console.WriteLine("==========================");

            return Task.CompletedTask;
        }
    }
}
