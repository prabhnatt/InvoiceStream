using InvoicingAPI.Services;
using InvoicingCore.Contracts.Invoices;
using InvoicingCore.Services;

namespace InvoicingAPI.Endpoints
{
    public static class InvoiceEndpoints
    {
        public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/invoices")
                .WithTags("Invoices");

            //POST /api/invoices
            group.MapPost("/", async (
                IUserContext userContext,
                InvoiceCreateRequest request,
                InvoiceService invoiceService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                try
                {
                    var invoice = await invoiceService.CreateInvoiceAsync(userId, request, ct);
                    var response = InvoiceService.ToResponse(invoice);
                    return Results.Created($"/api/invoices/{response.Id}", response);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            //GET /api/invoices
            group.MapGet("/", async (
                IUserContext userContext,
                InvoiceService invoiceService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                var invoices = await invoiceService.GetInvoicesForUserAsync(userId, ct);
                var response = invoices.Select(InvoiceService.ToResponse);
                return Results.Ok(response);
            });

            //GET /api/invoices/{id}
            group.MapGet("/{id}", async (
                IUserContext userContext,
                string id,
                InvoiceService invoiceService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                var invoice = await invoiceService.GetInvoiceByIdAsync(userId, id, ct);
                if (invoice is null)
                    return Results.NotFound();

                return Results.Ok(InvoiceService.ToResponse(invoice));
            });

            //PUT /api/invoices/{id}
            group.MapPut("/{id}", async (
                IUserContext userContext,
                string id,
                InvoiceCreateRequest request,
                InvoiceService invoiceService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                try
                {
                    var updated = await invoiceService.UpdateInvoiceAsync(userId, id, request, ct);
                    if (updated is null)
                        return Results.NotFound();

                    return Results.Ok(InvoiceService.ToResponse(updated));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            //DELETE /api/invoices/{id}
            group.MapDelete("/{id}", async (
                IUserContext userContext,
                string id,
                InvoiceService invoiceService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                var deleted = await invoiceService.DeleteInvoiceAsync(userId, id, ct);
                if (!deleted)
                    return Results.NotFound();

                return Results.NoContent();
            });

            return app;
        }
    }
}