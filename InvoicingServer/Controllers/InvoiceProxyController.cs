using InvoicingServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoicingServer.Controllers;

[Authorize]
[Route("invoices")]
public class InvoiceProxyController : Controller
{
    private readonly ApiClient _apiClient;
    private readonly IConfiguration _config;

    public InvoiceProxyController(ApiClient apiClient, IConfiguration config)
    {
        _apiClient = apiClient;
        _config = config;
    }

    //GET /invoices/{id}/pdf?style=minimal|business
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> GetInvoicePdf(string id, [FromQuery] string style = "minimal")
    {
        var apiBase = _config["ApiBaseUrl"] ?? "https://localhost:7134";

        //NOTE: calls the API, ApiClient will attach X-User-Id automatically
        var url = $"{apiBase}/api/invoices/{id}/pdf?style={style}";
        var response = await _apiClient.GetRawAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var text = await response.Content.ReadAsStringAsync();
            return StatusCode((int)response.StatusCode, text);
        }

        var bytes = await response.Content.ReadAsByteArrayAsync();

        var contentType = response.Content.Headers.ContentType?.MediaType
                          ?? "application/pdf";

        var fileName = "invoice.pdf";
        var cd = response.Content.Headers.ContentDisposition;
        if (cd != null)
        {
            fileName = cd.FileNameStar ?? cd.FileName ?? fileName;
        }

        return File(bytes, contentType, fileName);
    }
}
