using InvoicingUI;
using InvoicingUI.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// API base URL (adjust as needed)
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7134";

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

builder.Services.AddScoped<ClientApiService>();
builder.Services.AddScoped<InvoiceApiService>();

// Add a typed client that injects X-User-Id for now
builder.Services.AddScoped<ApiHttpClient>();

await builder.Build().RunAsync();
