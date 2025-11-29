namespace InvoicingUI.Services
{
    public class ApiHttpClient
    {
        private readonly HttpClient _http;

        // For now, we hardcode a dev user id. Later, this will come from auth.
        private const string DevUserId = "usr_dev_local";

        public ApiHttpClient(HttpClient http)
        {
            _http = http;
            if (!_http.DefaultRequestHeaders.Contains("X-User-Id"))
            {
                _http.DefaultRequestHeaders.Add("X-User-Id", DevUserId);
            }
        }

        public HttpClient Http => _http;
    }
}
