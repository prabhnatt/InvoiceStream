namespace InvoicingAPI.Services
{
    public class HeaderUserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HeaderUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId
        {
            get
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx is null) return null;

                if (ctx.Request.Headers.TryGetValue("X-User-Id", out var header) &&
                    !string.IsNullOrWhiteSpace(header))
                {
                    return header.ToString();
                }

                return null;
            }
        }
    }
}
