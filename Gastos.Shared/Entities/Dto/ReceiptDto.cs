namespace Gastos.Shared.Entities.Dto;
public sealed class ReceiptDto : EntityDto
{
    public Guid SourceId { get; set; } // Id of the source document (ReceiptResponse from DocIntel)
    public DateTime? TransactionDateUtc { get; set; }
    public Guid? StoreId { get; set; }
    public StoreDto? Store { get; set; }
    public decimal? Discount { get; set; }
    public List<ReceiptItemDto> Items { get; set; } = [];

    public decimal TotalAmount
    {
        get
        {
            decimal itemsTotal = Items.Sum(i => i.Amount);
            decimal discountAmount = Discount ?? 0.0m;
            return itemsTotal + discountAmount;
        }
    }
    public string TotalAmountFormatted => TotalAmount.ToStringAmount();
}
