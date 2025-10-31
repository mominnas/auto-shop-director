using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using MMN.Models;
using System.Linq;

public class InvoiceDocument : IDocument
{
    private readonly Order _order;
    private readonly string _companyName;
    private readonly string[] _companyAddress;
    private readonly string _companyPhone;
    private readonly string _taxId;

    public InvoiceDocument(Order order, string companyName, string[] companyAddress, string companyPhone, string taxId)
    {
        _order = order;
        _companyName = companyName;
        _companyAddress = companyAddress;
        _companyPhone = companyPhone;
        _taxId = taxId;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(40);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().AlignCenter().Text("Thank you for your business!");
        });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(_companyName).FontSize(20).Bold();
                foreach (var line in _companyAddress)
                    col.Item().Text(line);
                col.Item().Text(_companyPhone);
                col.Item().Text($"Tax ID: {_taxId}");
                col.Spacing(1);
            });
            row.ConstantItem(150).Height(80).Placeholder();
        });
    }

    void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10);

            
            // Top row: Customer info box (left) and Vehicle info box (right)
            col.Item().Row(row =>
            {
                // Customer info box (left)
                row.RelativeItem().Element(c => c
                    .Border(1)
                    .Padding(5)
                    .Column(customerCol =>
                    {
                        // Title at the top
                        customerCol.Item().Text("Customer Information").SemiBold().FontSize(12);

                        // Line under the title
                        customerCol.Item().LineHorizontal(1).LineColor(Colors.Black);

                        // Customer details
                        customerCol.Item().Text(_order.CustomerName ?? string.Empty).FontSize(10);
                        customerCol.Item().Text(_order.Address ?? string.Empty).FontSize(10);
                        customerCol.Item().Text(_order.Customer?.Phone ?? string.Empty).FontSize(10);
                        customerCol.Item().Text(_order.Customer?.Email ?? string.Empty).FontSize(10);
                    })
                );

                // Small spacer between boxes
                row.ConstantItem(0);

                // Vehicle info box (right)
                row.RelativeItem().Element(c => c
                    .Border(1)
                    .Padding(5)
                    .Column(vehicleCol =>
                    {
                        // Title at the top
                        vehicleCol.Item().Text("VEHICLE:").SemiBold().FontSize(12);

                        // Line under the title
                        vehicleCol.Item().LineHorizontal(1).LineColor(Colors.Black);

                        // Vehicle details
                        var vehicle = _order.Customer?.Vehicles?.FirstOrDefault();
                        vehicleCol.Item().Text(vehicle?.ToString() ?? "N/A").FontSize(10);
                        // If you have strongly-typed vehicle fields, display them here.
                    })
                );
            });

            col.Item().EnsureSpace(10);

            col.Item().PaddingBottom(5);

            // Invoice metadata
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(left =>
                {
                    left.Item().Text($"Invoice #: {_order.InvoiceNumber}").Bold();
                    
                });

                r.ConstantItem(200).Column(right =>
                {
                    //right.Item().AlignRight().Text($"Customer: {_order.CustomerName}");
                    //right.Item().AlignRight().Text($"Address: {_order.Address}");
                    right.Item().AlignRight().Text($"Date: {_order.DatePlaced:MM/dd/yyyy}");
                });
            });

            col.Item().PaddingTop(5);

            col.Item().EnsureSpace(5);

            // Line items table
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("Description").Bold();
                    header.Cell().AlignRight().Text("Quantity").Bold();
                    header.Cell().AlignRight().Text("Unit Price").Bold();
                    header.Cell().AlignRight().Text("Amount").Bold();
                });

                foreach (var item in _order.LineItems)
                {
                    table.Cell().Text(item.Product.Name);
                    table.Cell().AlignRight().Text(item.Quantity.ToString());
                    table.Cell().AlignRight().Text(item.Product.ListPrice.ToString("C"));
                    table.Cell().AlignRight().Text((item.Quantity * item.Product.ListPrice).ToString("C"));
                }
            });
            col.Item().Text("");
            col.Item().AlignRight().Text($"Subtotal: {_order.Subtotal:C}");
            col.Item().AlignRight().Text($"Tax: {_order.Tax:C}");
            col.Item().AlignRight().Text($"Total: {_order.GrandTotal:C}").Bold();
        });
    }
}