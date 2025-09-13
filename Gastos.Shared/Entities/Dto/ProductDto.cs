namespace Gastos.Shared.Entities.Dto;

public sealed class ProductDto : EntityDto
{
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int UnitsPack { get; set; } = 1;
    public int? SizingId { get; set; } = 4;
    public decimal? SizingValue { get; set; }
    public SizingDto? Sizing { get; set; }
    public List<ReceiptItemDto> ReceiptItems { get; set; } = [];


    public bool SizingIsValid
    {
        get => Sizing is not null && SizingValue.HasValue && !(Sizing.Id == 6 && SizingValue == 1);
    }

    public string SizingDescription
    {
        get
        {
            if (!SizingIsValid) return string.Empty;

            decimal val = SizingValue!.Value;
            string valFormatted = val.ToStringQuantity();

            return $"{valFormatted} {Sizing!.Name}";
        }
    }

    public string FullName
    {
        get => $"{Name.Trim()}{(SizingIsValid ? ", " + SizingDescription : "")}{(UnitsPack > 1 ? $", pack {UnitsPack}" : "")}".Trim();
    }
}
