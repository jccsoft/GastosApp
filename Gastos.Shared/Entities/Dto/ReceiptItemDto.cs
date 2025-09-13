namespace Gastos.Shared.Entities.Dto;

public sealed class ReceiptItemDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReceiptId { get; set; }
    public ReceiptDto Receipt { get; set; } = null!;
    public Guid? ProductId { get; set; }
    public ProductDto? Product { get; set; }
    public string? SourceDescription { get; set; }
    public decimal Quantity { get; set; } = 1.0m;
    public decimal Amount { get; set; } = 0.0m;


    public decimal UnitPrice => Quantity == 0 ? 0 : Amount / Quantity;
    public string UnitPriceFormatted => UnitPrice.ToStringAmount();

    public string QuantityFormattedWithOne => Quantity.ToStringQuantity();
    public string QuantityFormattedWithoutOne
    {
        get
        {
            if (Quantity == 1) return "";
            return QuantityFormattedWithOne;
        }
    }

    public string AmountFormatted => Amount.ToStringAmount();

    public string ProductDescription
    {
        get
        {
            if (Product is null) return string.Empty;

            return $"{(Quantity == 1 ? "" : QuantityFormattedWithoutOne)} {Product!.Name}".Trim();
        }
    }

}
