using InvoicingCore;
using InvoicingCore.Configuration;
using InvoicingCore.Services;
using InvoicingServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// Razor + Blazor Server
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers(); // for AuthController

builder.Services.AddHttpContextAccessor();

//Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.AccessDeniedPath = "/auth/denied";
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";

        options.CallbackPath = "/signin-google";
        options.Scope.Add("email");
        options.Scope.Add("profile");
        options.SaveTokens = true;

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async ctx =>
            {
                var httpContext = ctx.HttpContext;
                var userService = httpContext.RequestServices.GetRequiredService<UserService>();

                var subject = ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value
                            ?? ctx.Principal?.FindFirst("email")?.Value;
                var name = ctx.Principal?.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(subject))
                    return;

                // Map Google -> internal user (and persist in Mongo)
                var user = await userService.GetOrCreateFromExternalAsync(
                    provider: "google",
                    subject: subject,
                    email: email,
                    displayName: name,
                    ct: httpContext.RequestAborted);

                // Add *your* user id into auth cookie
                var identity = (ClaimsIdentity)ctx.Principal!.Identity!;
                identity.AddClaim(new Claim("userId", user.Id));
            }
        };
    });

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("Mongo"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserService>();

// HttpClient for calling API
builder.Services.AddHttpClient<ApiClient>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var apiBase = config["ApiBaseUrl"];
    client.BaseAddress = new Uri(apiBase);
});

//Authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
    //TODO: Custom Policies?
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
