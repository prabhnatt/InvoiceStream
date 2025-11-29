using InvoicingUI.Models;
using System.Net.Http.Json;

namespace InvoicingUI.Services
{
    public class ClientApiService
    {
        private readonly ApiHttpClient _api;

        public ClientApiService(ApiHttpClient api)
        {
            _api = api;
        }

        public async Task<List<ClientResponse>> GetClientsAsync()
        {
            var result = await _api.Http.GetFromJsonAsync<List<ClientResponse>>("/api/clients");
            return result ?? new List<ClientResponse>();
        }

        public async Task<ClientResponse?> CreateClientAsync(ClientCreateRequest request)
        {
            var response = await _api.Http.PostAsJsonAsync("/api/clients", request);
            if (!response.IsSuccessStatusCode)
            {
                // handle / surface error (for now, throw)
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create client: {error}");
            }

            return await response.Content.ReadFromJsonAsync<ClientResponse>();
        }
    }
}
