using InvoicingAPI.Services;
using InvoicingCore;
using InvoicingCore.Contracts.Invoices;
using InvoicingCore.Enums;
using InvoicingCore.Models;
using InvoicingCore.Pdf;
using InvoicingCore.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace InvoicingAPI.Endpoints
{
    public static class InvoiceEndpoints
    {
        public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/invoices")
                .WithTags("Invoices");

            //MARK SENT
            group.MapPost("/{id}/mark-sent", MarkSentAsync);

            //MARK PAID
            group.MapPost("/{id}/mark-paid", MarkPaidAsync);

            //POST /api/invoices
            group.MapPost("/", async (
                HttpContext http,
                InvoiceCreateRequest request,
                MongoDbContext db,
                BusinessProfileService profileService,
                InvoiceNumberService invoiceNumberService,
                CancellationToken ct) =>
            {
                var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                var profile = await profileService.GetOrCreateForUserAsync(userId, ct);

                //Apply defaults if not sent from UI
                var currency = string.IsNullOrWhiteSpace(request.Currency)
                    ? profile.DefaultCurrency
                    : request.Currency;

                var issueDate = request.IssueDate == default
                    ? DateOnly.FromDateTime(DateTime.UtcNow)
                    : request.IssueDate;

                var dueDate = request.DueDate == default
                    ? issueDate.AddDays(profile.DefaultPaymentTermsDays)
                    : request.DueDate;

                //Apply tax default where line items have 0 tax
                foreach (var li in request.LineItems)
                {
                    if (li.TaxRate <= 0)
                        li.TaxRate = profile.DefaultTaxRate;
                }

                //Recalculate totals on server to be safe
                decimal subTotal = 0;
                decimal taxTotal = 0;
                foreach (var li in request.LineItems)
                {
                    var baseAmount = li.Quantity * li.UnitPrice;
                    var lineTax = baseAmount * li.TaxRate;
                    subTotal += baseAmount;
                    taxTotal += lineTax;
                }

                request.Totals.SubTotal = subTotal;
                request.Totals.Tax = taxTotal;
                request.Totals.GrandTotal = subTotal + taxTotal;

                var invoice = new Invoice
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    UserId = userId,
                    ClientId = request.ClientId,
                    InvoiceNumber = await invoiceNumberService.GetNextInvoiceNumberAsync(userId, ct),
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    Currency = currency,
                    Status = request.Status,
                    Type = request.Type,
                    LineItems = request.LineItems.Select(li => new InvoiceLineItem
                    {
                        Description = li.Description,
                        Quantity = li.Quantity,
                        UnitPrice = li.UnitPrice,
                        TaxRate = li.TaxRate
                    }).ToList(),
                    Totals = new InvoiceTotals
                    {
                        SubTotal = request.Totals.SubTotal,
                        Tax = request.Totals.Tax,
                        GrandTotal = request.Totals.GrandTotal
                    },
                    Notes = request.Notes
                };

                await db.Invoices.InsertOneAsync(invoice, cancellationToken: ct);

                return Results.Ok(MapToResponse(invoice));
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

            //GET /api/invoices/{id}/pdf
            group.MapGet("/{id}/pdf", async (
                string id,
                HttpContext http,
                InvoicePdfService pdfService,
                CancellationToken ct) =>
            {
                var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(userId))
                    return Results.Unauthorized();

                var styleParam = http.Request.Query["style"].ToString();
                var style = styleParam?.ToLowerInvariant() switch
                {
                    "business" => InvoicePdfStyle.Business,
                    _ => InvoicePdfStyle.Minimal
                };

                byte[] pdfBytes;
                try
                {
                    pdfBytes = await pdfService.GenerateInvoicePdfAsync(id, style, userId, ct);
                }
                catch (InvalidOperationException)
                {
                    return Results.NotFound();
                }

                var fileName = $"Invoice_{id}_{style}.pdf";
                return Results.File(
                    pdfBytes,
                    "application/pdf",
                    fileName);
            })
            .WithName("GetInvoicePdf")
.Produces(200, contentType: "application/pdf");

            return app;
        }

        private static async Task<IResult> MarkSentAsync(
       string id,
       HttpContext http,
       MongoDbContext db,
       CancellationToken ct)
        {
            var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();

            var filter = Builders<Invoice>.Filter.Where(x => x.Id == id && x.UserId == userId);

            var update = Builders<Invoice>.Update
                .Set(x => x.Status, InvoiceStatus.Sent)
                .Set(x => x.SentAtUtc, DateTime.UtcNow);

            var options = new FindOneAndUpdateOptions<Invoice>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updated = await db.Invoices.FindOneAndUpdateAsync(filter, update, options, ct);

            if (updated is null)
                return Results.NotFound();

            return Results.Ok(MapToResponse(updated));
        }

        //---------------- MARK PAID ----------------

        private static async Task<IResult> MarkPaidAsync(
            string id,
            HttpContext http,
            MarkPaidRequest request,
            MongoDbContext db,
            CancellationToken ct)
        {
            var userId = http.Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Results.Unauthorized();

            var filter = Builders<Invoice>.Filter.Where(x => x.Id == id && x.UserId == userId);

            var update = Builders<Invoice>.Update
                .Set(x => x.Status, InvoiceStatus.Paid)
                .Set(x => x.PaidAtUtc, DateTime.UtcNow)
                .Set(x => x.PaymentMethod, request.PaymentMethod)
                .Set(x => x.PaymentReference, request.PaymentReference);

            var options = new FindOneAndUpdateOptions<Invoice>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updated = await db.Invoices.FindOneAndUpdateAsync(filter, update, options, ct);

            if (updated is null)
                return Results.NotFound();

            return Results.Ok(MapToResponse(updated));
        }

        //---------------- MAPPER ----------------

        private static InvoiceResponse MapToResponse(Invoice invoice)
        {
            return new InvoiceResponse
            {
                Id = invoice.Id,
                ClientId = invoice.ClientId,
                InvoiceNumber = invoice.InvoiceNumber,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Currency = invoice.Currency,
                Status = invoice.Status,
                Type = invoice.Type,
                Notes = invoice.Notes,

                LineItems = invoice.LineItems.Select(li => new InvoiceLineItem
                {
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    TaxRate = li.TaxRate
                }).ToList(),

                Totals = new InvoiceTotals
                {
                    SubTotal = invoice.Totals.SubTotal,
                    Tax = invoice.Totals.Tax,
                    GrandTotal = invoice.Totals.GrandTotal
                },

                SentAtUtc = invoice.SentAtUtc,
                PaidAtUtc = invoice.PaidAtUtc,
                PaymentMethod = invoice.PaymentMethod,
                PaymentReference = invoice.PaymentReference
            };
        }
    }

}
