namespace Gastos.Shared.Entities.Dto;

public sealed class StoreDto : EntityDto
{
    public string Name { get; set; } = string.Empty;
    public string? SourceName { get; set; }
    public List<ReceiptDto> Receipts { get; set; } = [];
}
