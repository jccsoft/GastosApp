namespace Gastos.Api.Features.Receipts;

public sealed class ReceiptItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReceiptId { get; set; }
    [JsonIgnore]
    public Receipt Receipt { get; set; } = null!;
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? SourceDescription { get; set; }
    public decimal Quantity { get; set; } = 1.0m;
    public decimal Amount { get; set; } = 0.0m;
}
