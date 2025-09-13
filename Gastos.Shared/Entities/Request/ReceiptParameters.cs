namespace Gastos.Shared.Entities.Request;

public sealed class ReceiptParameters : PaginationParameters
{
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; } = string.Empty;
    public DateTimeOffset? FromDateUtc { get; set; }
    public DateTimeOffset? ToDateUtc { get; set; }
}
