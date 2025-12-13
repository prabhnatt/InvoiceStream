using System.Security.Claims;

namespace InvoicingServer.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<HttpResponseMessage> GetRawAsync(string uri)
        {
            EnsureUserHeader();
            return _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);
        }

        private void EnsureUserHeader()
        {
            var ctx = _httpContextAccessor.HttpContext;
            var userId =
                ctx?.User.FindFirst("userId")?.Value ??
                ctx?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _httpClient.DefaultRequestHeaders.Remove("X-User-Id");

            if (!string.IsNullOrWhiteSpace(userId))
            {
                _httpClient.DefaultRequestHeaders.Add("X-User-Id", userId);
            }
        }

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            EnsureUserHeader();
            return await _httpClient.GetAsync(uri);
        }

        public async Task<HttpResponseMessage> PostEmptyAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent("", System.Text.Encoding.UTF8, "application/json")
            };

            return await _httpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> PostAsJsonAsync<T>(string uri, T value)
        {
            EnsureUserHeader();
            return await _httpClient.PostAsJsonAsync(uri, value);
        }

        public async Task<HttpResponseMessage> PutAsJsonAsync<T>(string uri, T value)
        {
            EnsureUserHeader();
            return await _httpClient.PutAsJsonAsync(uri, value);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string uri)
        {
            EnsureUserHeader();
            return await _httpClient.DeleteAsync(uri);
        }
    }
}
