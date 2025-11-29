using InvoicingUI.Models;
using System.Net.Http.Json;

namespace InvoicingUI.Services
{
    public class InvoiceApiService
    {
        private readonly ApiHttpClient _api;

        public InvoiceApiService(ApiHttpClient api)
        {
            _api = api;
        }

        public async Task<List<InvoiceResponse>> GetInvoicesAsync()
        {
            var result = await _api.Http.GetFromJsonAsync<List<InvoiceResponse>>("/api/invoices");
            return result ?? new List<InvoiceResponse>();
        }

        public async Task<InvoiceResponse?> CreateInvoiceAsync(InvoiceCreateRequest request)
        {
            var response = await _api.Http.PostAsJsonAsync("/api/invoices", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Failed to create invoice: {error}");
            }

            return await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        }
    }
}
