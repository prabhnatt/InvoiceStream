using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InvoicingCore.Pdf;

public class InvoicePdfDocument : IDocument
{
    private readonly InvoicePdfModel _model;
    private readonly InvoicePdfStyle _style;

    public InvoicePdfDocument(InvoicePdfModel model, InvoicePdfStyle style)
    {
        _model = model;
        _style = style;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Grey.Darken3));

            //Shared background / header style per template
            if (_style == InvoicePdfStyle.Minimal)
            {
                page.PageColor(Colors.White);
            }
            else
            {
                page.PageColor(Colors.Grey.Lighten5);
            }

            page.Content()
                .Column(col =>
                {
                    if (_style == InvoicePdfStyle.Minimal)
                        ComposeMinimal(col);
                    else
                        ComposeBusiness(col);
                });
        });
    }

    //---------------- Minimal / Stripe-style ----------------
    private void ComposeMinimal(ColumnDescriptor col)
    {
        col.Spacing(18);

        //HEADER: Business info + invoice meta
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                //Business name
                c.Item().Text(_model.BusinessName)
                    .SemiBold().FontSize(16)
                    .FontColor(Colors.Blue.Medium);

                if (!string.IsNullOrWhiteSpace(_model.BusinessLegalName) &&
                    !_model.BusinessLegalName.Equals(_model.BusinessName, StringComparison.OrdinalIgnoreCase))
                {
                    c.Item().Text(_model.BusinessLegalName)
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.BusinessTaxNumber))
                {
                    c.Item().Text($"HST/GST: {_model.BusinessTaxNumber}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.BusinessAddress))
                {
                    c.Item().Text(_model.BusinessAddress)
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }

                //contact line
                var contactBits = new[]
                {
                _model.BusinessEmail,
                _model.BusinessPhone,
                _model.BusinessWebsite
            }.Where(x => !string.IsNullOrWhiteSpace(x));

                if (contactBits.Any())
                {
                    c.Item().Text(string.Join(" · ", contactBits))
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }
            });

            row.ConstantItem(150).Column(c =>
            {
                c.Item().AlignRight().Text("INVOICE")
                    .SemiBold().FontSize(15).FontColor(Colors.Blue.Darken2);
                c.Item().AlignRight().Text($"Invoice #: {_model.InvoiceNumber}")
                    .FontSize(10);
                c.Item().AlignRight().Text($"Status: {_model.Status}")
                    .FontSize(10)
                    .FontColor(GetStatusColor());
                c.Item().AlignRight().Text($"Issue: {_model.IssueDate:yyyy-MM-dd}")
                    .FontSize(9);
                c.Item().AlignRight().Text($"Due:   {_model.DueDate:yyyy-MM-dd}")
                    .FontSize(9);
                c.Item().AlignRight().Text($"Currency: {_model.Currency}")
                    .FontSize(9);
            });
        });

        //BILL TO: single, rich client block
        col.Item().Column(c =>
        {
            c.Item().Text("Bill To").SemiBold().FontSize(10);

            c.Item().Text(_model.ClientName);

            if (!string.IsNullOrWhiteSpace(_model.ClientLegalName) &&
                !_model.ClientLegalName.Equals(_model.ClientName, StringComparison.OrdinalIgnoreCase))
            {
                c.Item().Text(_model.ClientLegalName)
                    .FontSize(9).FontColor(Colors.Grey.Darken2);
            }

            if (!string.IsNullOrWhiteSpace(_model.ClientTaxNumber))
            {
                c.Item().Text($"Tax ID: {_model.ClientTaxNumber}")
                    .FontSize(9).FontColor(Colors.Grey.Darken2);
            }

            if (!string.IsNullOrWhiteSpace(_model.ClientAddress))
            {
                c.Item().Text(_model.ClientAddress)
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
            }

            if (!string.IsNullOrWhiteSpace(_model.ClientContactName))
            {
                var contactLine = _model.ClientContactName;
                if (!string.IsNullOrWhiteSpace(_model.ClientContactRole))
                    contactLine += $" ({_model.ClientContactRole})";

                c.Item().Text(contactLine)
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
            }

            var clientContactBits = new[]
            {
            _model.ClientEmail,
            _model.ClientPhone
        }.Where(x => !string.IsNullOrWhiteSpace(x));

            if (clientContactBits.Any())
            {
                c.Item().Text(string.Join(" · ", clientContactBits))
                    .FontSize(8).FontColor(Colors.Grey.Darken2);
            }
        });

        //LINE ITEMS
        col.Item().Element(ComposeLineItemsTable);

        //TOTALS
        col.Item().AlignRight().Column(c =>
        {
            c.Spacing(2);
            c.Item().Row(r =>
            {
                r.RelativeItem().Text("Subtotal");
                r.ConstantItem(90).AlignRight().Text(FormatMoney(_model.SubTotal));
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text("Tax");
                r.ConstantItem(90).AlignRight().Text(FormatMoney(_model.Tax));
            });
            c.Item().PaddingTop(6).Row(r =>
            {
                r.RelativeItem().Text("Total").SemiBold();
                r.ConstantItem(90).AlignRight().Text(FormatMoney(_model.Total))
                    .SemiBold().FontSize(12);
            });
        });

        //NOTES
        if (!string.IsNullOrWhiteSpace(_model.Notes))
        {
            col.Item().PaddingTop(10).Column(c =>
            {
                c.Item().Text("Notes").SemiBold().FontSize(10);
                c.Item().Text(_model.Notes).FontSize(9).FontColor(Colors.Grey.Darken2);
            });
        }

        //PAYMENT INSTRUCTIONS
        if (!string.IsNullOrWhiteSpace(_model.PaymentInstructions))
        {
            col.Item().PaddingTop(10).Column(c =>
            {
                c.Item().Text("Payment Instructions").SemiBold().FontSize(10);
                c.Item().Text(_model.PaymentInstructions).FontSize(9).FontColor(Colors.Grey.Darken2);
            });
        }

        col.Item().PaddingTop(20).Text("Thank you for your business.")
            .FontSize(9).FontColor(Colors.Grey.Darken2);
    }


    //---------------- Business ----------------
    private void ComposeBusiness(ColumnDescriptor col)
    {
        col.Spacing(18);

        //HEADER: Business info + invoice meta
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text(_model.BusinessName)
                    .SemiBold().FontSize(16)
                    .FontColor(Colors.Blue.Medium);

                if (!string.IsNullOrWhiteSpace(_model.BusinessLegalName) &&
                    !_model.BusinessLegalName.Equals(_model.BusinessName, StringComparison.OrdinalIgnoreCase))
                {
                    c.Item().Text(_model.BusinessLegalName)
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.BusinessTaxNumber))
                {
                    c.Item().Text($"HST/GST: {_model.BusinessTaxNumber}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.BusinessAddress))
                {
                    c.Item().Text(_model.BusinessAddress)
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }

                var contactBits = new[]
                {
                _model.BusinessEmail,
                _model.BusinessPhone,
                _model.BusinessWebsite
            }.Where(x => !string.IsNullOrWhiteSpace(x));

                if (contactBits.Any())
                {
                    c.Item().Text(string.Join(" · ", contactBits))
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }
            });

            row.ConstantItem(170).Column(c =>
            {
                c.Item().AlignRight().Text("INVOICE")
                    .SemiBold().FontSize(15).FontColor(Colors.Blue.Darken2);
                c.Item().AlignRight().Text($"Invoice #{_model.InvoiceNumber}")
                    .SemiBold().FontSize(14);
                c.Item().AlignRight().Text($"Status: {_model.Status}")
                    .FontSize(10)
                    .FontColor(GetStatusColor());
                c.Item().AlignRight().Text($"Issue: {_model.IssueDate:yyyy-MM-dd}").FontSize(9);
                c.Item().AlignRight().Text($"Due:   {_model.DueDate:yyyy-MM-dd}").FontSize(9);
            });
        });

        //ROW: Bill To (left) + Invoice Details (right)
        col.Item().Row(row =>
        {
            row.RelativeItem().Column(c =>
            {
                c.Item().Text("Bill To").SemiBold().FontSize(10);

                c.Item().Text(_model.ClientName);

                if (!string.IsNullOrWhiteSpace(_model.ClientLegalName) &&
                    !_model.ClientLegalName.Equals(_model.ClientName, StringComparison.OrdinalIgnoreCase))
                {
                    c.Item().Text(_model.ClientLegalName)
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.ClientTaxNumber))
                {
                    c.Item().Text($"Tax ID: {_model.ClientTaxNumber}")
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.ClientAddress))
                {
                    c.Item().Text(_model.ClientAddress)
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.ClientContactName))
                {
                    var contactLine = _model.ClientContactName;
                    if (!string.IsNullOrWhiteSpace(_model.ClientContactRole))
                        contactLine += $" ({_model.ClientContactRole})";

                    c.Item().Text(contactLine)
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }

                var clientContactBits = new[]
                {
                _model.ClientEmail,
                _model.ClientPhone
            }.Where(x => !string.IsNullOrWhiteSpace(x));

                if (clientContactBits.Any())
                {
                    c.Item().Text(string.Join(" · ", clientContactBits))
                        .FontSize(8).FontColor(Colors.Grey.Darken2);
                }
            });

            row.ConstantItem(170).Column(c =>
            {
                c.Item().Text("Invoice Details").SemiBold().FontSize(10);
                c.Item().Text($"Issue Date: {_model.IssueDate:yyyy-MM-dd}").FontSize(9);
                c.Item().Text($"Due Date:   {_model.DueDate:yyyy-MM-dd}").FontSize(9);
                c.Item().Text($"Currency:   {_model.Currency}").FontSize(9);
            });
        });

        //LINE ITEMS
        col.Item().Element(ComposeLineItemsTable);

        //TOTALS
        col.Item().AlignRight().Column(c =>
        {
            c.Spacing(2);
            c.Item().Row(r =>
            {
                r.RelativeItem().Text("Subtotal");
                r.ConstantItem(100).AlignRight().Text(FormatMoney(_model.SubTotal));
            });
            c.Item().Row(r =>
            {
                r.RelativeItem().Text("Tax");
                r.ConstantItem(100).AlignRight().Text(FormatMoney(_model.Tax));
            });
            c.Item().PaddingTop(6).Row(r =>
            {
                r.RelativeItem().Text("Total").SemiBold();
                r.ConstantItem(100).AlignRight().Text(FormatMoney(_model.Total))
                    .SemiBold().FontSize(13);
            });
        });

        //PAYMENT INSTRUCTIONS
        if (!string.IsNullOrWhiteSpace(_model.PaymentInstructions))
        {
            col.Item().PaddingTop(10).Column(c =>
            {
                c.Item().Text("Payment Instructions")
                    .SemiBold().FontSize(10);
                c.Item().Text(_model.PaymentInstructions)
                    .FontSize(9).FontColor(Colors.Grey.Darken2);
            });
        }

        //NOTES (per-invoice + default)
        if (!string.IsNullOrWhiteSpace(_model.Notes) ||
            !string.IsNullOrWhiteSpace(_model.DefaultInvoiceNotes))
        {
            col.Item().PaddingTop(8).Column(c =>
            {
                c.Item().Text("Notes")
                    .SemiBold().FontSize(10);

                if (!string.IsNullOrWhiteSpace(_model.Notes))
                {
                    c.Item().Text(_model.Notes)
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }

                if (!string.IsNullOrWhiteSpace(_model.DefaultInvoiceNotes))
                {
                    c.Item().Text(_model.DefaultInvoiceNotes)
                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                }
            });
        }

        col.Item().PaddingTop(18).Text("If you have any questions about this invoice, please contact us.")
            .FontSize(9).FontColor(Colors.Grey.Darken2);
    }


    //---------------- Shared table ----------------
    private void ComposeLineItemsTable(QuestPDF.Infrastructure.IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4); //description
                columns.RelativeColumn(1); //qty
                columns.RelativeColumn(2); //unit price
                columns.RelativeColumn(2); //tax
                columns.RelativeColumn(2); //line total
            });

            //Header
            table.Header(header =>
            {
                header.Cell().Element(HeaderCell).Text("Description");
                header.Cell().Element(HeaderCell).AlignRight().Text("Qty");
                header.Cell().Element(HeaderCell).AlignRight().Text("Unit");
                header.Cell().Element(HeaderCell).AlignRight().Text("Tax");
                header.Cell().Element(HeaderCell).AlignRight().Text("Total");
            });

            //Rows
            foreach (var item in _model.LineItems)
            {
                table.Cell().Element(Cell).Text(item.Description);
                table.Cell().Element(Cell).AlignRight().Text(item.Quantity.ToString("0.##"));
                table.Cell().Element(Cell).AlignRight().Text(FormatMoney(item.UnitPrice));
                table.Cell().Element(Cell).AlignRight().Text($"{item.TaxRate:P0}");
                table.Cell().Element(Cell).AlignRight().Text(FormatMoney(item.LineTotal));
            }
        });

        static QuestPDF.Infrastructure.IContainer HeaderCell(QuestPDF.Infrastructure.IContainer container) =>
            container.DefaultTextStyle(x => x.SemiBold().FontSize(9))
                     .PaddingVertical(4)
                     .BorderBottom(1)
                     .BorderColor(Colors.Grey.Lighten2);

        static QuestPDF.Infrastructure.IContainer Cell(QuestPDF.Infrastructure.IContainer container) =>
            container.PaddingVertical(3)
                     .BorderBottom(0.5f)
                     .BorderColor(Colors.Grey.Lighten4);
        }

    private string FormatMoney(decimal value) =>
        $"{_model.Currency} {value:0.00}";

    private string GetStatusColor()
    {
        //var status = _model.Status ?? "";
        return _model.Status switch
        {
            Enums.InvoiceStatus.Draft => Colors.Green.Darken2,
            Enums.InvoiceStatus.Overdue => Colors.Red.Darken2,
            Enums.InvoiceStatus.Sent => Colors.Blue.Darken2,
            Enums.InvoiceStatus.Void => Colors.Grey.Darken2,
            _ => Colors.Grey.Darken2
        };
    }
}
