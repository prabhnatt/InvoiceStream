using InvoicingAPI.Services;
using InvoicingCore;
using InvoicingCore.Contracts.Clients;

namespace InvoicingAPI.Endpoints
{
    public static class ClientEndpoints
    {
        public static IEndpointRouteBuilder MapClientEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/clients")
                .WithTags("Clients");

            //POST /api/clients
            group.MapPost("/", async (
                IUserContext userContext,
                ClientCreateRequest request,
                ClientService clientService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                try
                {
                    var client = await clientService.CreateClientAsync(userId, request, ct);
                    var response = ClientService.ToResponse(client);
                    return Results.Created($"/api/clients/{response.Id}", response);
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            //GET /api/clients
            group.MapGet("/", async (
                IUserContext userContext,
                ClientService clientService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                var clients = await clientService.GetClientsForUserAsync(userId, ct);
                var response = clients.Select(ClientService.ToResponse);
                return Results.Ok(response);
            });

            //GET /api/clients/{id}
            group.MapGet("/{id}", async (
                IUserContext userContext,
                string id,
                ClientService clientService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                var client = await clientService.GetClientByIdAsync(userId, id, ct);
                if (client is null)
                    return Results.NotFound();

                return Results.Ok(ClientService.ToResponse(client));
            });

            //PUT /api/clients/{id}
            group.MapPut("/{id}", async (
                IUserContext userContext,
                string id,
                ClientCreateRequest request,
                ClientService clientService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                try
                {
                    var updated = await clientService.UpdateClientAsync(userId, id, request, ct);
                    if (updated is null)
                        return Results.NotFound();

                    return Results.Ok(ClientService.ToResponse(updated));
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(ex.Message);
                }
            });

            //DELETE /api/clients/{id}
            group.MapDelete("/{id}", async (
                IUserContext userContext,
                string id,
                ClientService clientService,
                CancellationToken ct) =>
            {
                var userId = userContext.UserId;
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.BadRequest("Missing X-User-Id header.");

                var deleted = await clientService.DeleteClientAsync(userId, id, ct);
                if (!deleted)
                    return Results.NotFound();

                return Results.NoContent();
            });

            return app;
        }
    }
}