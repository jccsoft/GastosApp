namespace Gastos.Shared.Entities.Dto;

public sealed class ReceiptDto : EntityDto
{
    public Guid SourceId { get; set; } // Id of the source document (ReceiptResponse from DocIntel)
    public DateTime? TransactionDateUtc { get; set; }
    public Guid? StoreId { get; set; }
    public StoreDto? Store { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = [];
}
