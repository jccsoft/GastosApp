namespace Gastos.Shared.Entities.Request;

public sealed class StoreParameters : PaginationParameters
{
    public string SearchString { get; set; } = string.Empty;
}