namespace Gastos.Shared.Entities.Request;

public sealed class ProductParameters : PaginationParameters
{
    public string SearchString { get; set; } = string.Empty;
    public bool EmptyImageUrl { get; set; } = false;
}
