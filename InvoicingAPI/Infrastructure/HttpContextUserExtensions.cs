namespace InvoicingAPI.Infrastructure
{
    public static class HttpContextUserExtensions
    {
        public static string? GetUserIdOrNull(this HttpContext context)
        {
            return context.User.FindFirst("userId")?.Value;
        }
    }
}
