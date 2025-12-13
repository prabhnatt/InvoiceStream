using InvoicingAPI.Services;
using InvoicingCore;
using InvoicingCore.Contracts.Clients;
using InvoicingCore.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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
                HttpContext http,
                ClientCreateRequest request,
                MongoDbContext db,
                CancellationToken ct) =>
            {
                var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                var client = new Client
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = userId,
                    Name = request.Name,
                    LegalName = request.LegalName,
                    TaxNumber = request.TaxNumber,
                    Address = request.Address,
                    PrimaryContactName = request.PrimaryContactName,
                    PrimaryContactRole = request.PrimaryContactRole,
                    PrimaryEmail = request.PrimaryEmail,
                    PrimaryPhone = request.PrimaryPhone,
                    DefaultCurrency = request.DefaultCurrency,
                    DefaultPaymentTermsDays = request.DefaultPaymentTermsDays,
                    DefaultTaxRate = request.DefaultTaxRate,
                    Notes = request.Notes
                };

                await db.Clients.InsertOneAsync(client, cancellationToken: ct);

                return Results.Ok(MapToClientResponse(client));
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
                string id,
                HttpContext http,
                ClientUpdateRequest request,
                MongoDbContext db,
                CancellationToken ct) =>
            {
                var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                if (id != request.Id)
                    return Results.BadRequest("Id mismatch.");

                var update = Builders<Client>.Update
                    .Set(x => x.Name, request.Name)
                    .Set(x => x.LegalName, request.LegalName)
                    .Set(x => x.TaxNumber, request.TaxNumber)
                    .Set(x => x.Address, request.Address)
                    .Set(x => x.PrimaryContactName, request.PrimaryContactName)
                    .Set(x => x.PrimaryContactRole, request.PrimaryContactRole)
                    .Set(x => x.PrimaryEmail, request.PrimaryEmail)
                    .Set(x => x.PrimaryPhone, request.PrimaryPhone)
                    .Set(x => x.DefaultCurrency, request.DefaultCurrency)
                    .Set(x => x.DefaultPaymentTermsDays, request.DefaultPaymentTermsDays)
                    .Set(x => x.DefaultTaxRate, request.DefaultTaxRate)
                    .Set(x => x.Notes, request.Notes);

                var result = await db.Clients.FindOneAndUpdateAsync(
                    x => x.Id == id && x.UserId == userId,
                    update,
                    new FindOneAndUpdateOptions<Client>
                    {
                        ReturnDocument = ReturnDocument.After
                    },
                    ct);

                if (result is null)
                    return Results.NotFound();

                return Results.Ok(MapToClientResponse(result));
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

        private static ClientResponse MapToClientResponse(Client c)
        {
            return new ClientResponse
            {
                Id = c.Id,
                Name = c.Name,
                LegalName = c.LegalName,
                TaxNumber = c.TaxNumber,
                Address = c.Address,
                PrimaryContactName = c.PrimaryContactName,
                PrimaryContactRole = c.PrimaryContactRole,
                PrimaryEmail = c.PrimaryEmail,
                PrimaryPhone = c.PrimaryPhone,
                DefaultCurrency = c.DefaultCurrency,
                DefaultPaymentTermsDays = c.DefaultPaymentTermsDays,
                DefaultTaxRate = c.DefaultTaxRate,
                Notes = c.Notes
            };
        }

    }
}