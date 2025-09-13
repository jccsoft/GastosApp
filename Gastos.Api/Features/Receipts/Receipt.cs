namespace Gastos.Api.Features.Receipts;

public sealed class Receipt : Entity
{
    public Guid SourceId { get; set; } // Id of the source document (ReceiptResponse from DocIntel)
    public DateTime? TransactionDateUtc { get; set; }
    public Guid? StoreId { get; set; }
    public Store? Store { get; set; }
    public List<ReceiptItem> Items { get; set; } = [];
}
