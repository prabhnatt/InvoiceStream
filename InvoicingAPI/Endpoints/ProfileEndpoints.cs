using InvoicingAPI.Services;
using InvoicingCore.Models;

namespace InvoicingAPI.Endpoints
{
    public static class ProfileEndpoints
    {

        public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
        {
            var profileGroup = app.MapGroup("/api/profile")
                .WithTags("Profile"); ;

            profileGroup.MapGet("/me", async (
                HttpContext http,
                BusinessProfileService service,
                CancellationToken ct) =>
            {
                var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                var profile = await service.GetOrCreateForUserAsync(userId, ct);
                return Results.Ok(profile);
            });

            profileGroup.MapPut("/me", async (
                HttpContext http,
                BusinessProfileService service,
                BusinessProfile profile,
                CancellationToken ct) =>
            {
                var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                var saved = await service.UpdateForUserAsync(userId, profile, ct);
                return Results.Ok(saved);
            });

            return app;
        }
    }
}
