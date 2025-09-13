namespace Gastos.Api.Features.Stores;

public sealed class Store : Entity
{
    public string Name { get; set; } = string.Empty;
    public string? SourceName { get; set; }

    [JsonIgnore]
    public List<Receipt> Receipts { get; set; } = [];
}
