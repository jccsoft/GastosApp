namespace Gastos.Api.Features.Products;

public sealed class Product : Entity
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int UnitsPack { get; set; } = 1;
    public int? SizingId { get; set; } = 4;
    public decimal? SizingValue { get; set; }
    public Sizing? Sizing { get; set; }

    [JsonIgnore]
    public List<ReceiptItem> ReceiptItems { get; set; } = [];
}
