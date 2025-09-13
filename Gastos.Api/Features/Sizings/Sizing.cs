namespace Gastos.Api.Features.Sizings;

public sealed class Sizing
{
    public int Id { get; set; } // 1=ml, 2=cl, 3=l, 4=g, 5=kg, 6=u
    public string Name { get; set; } = string.Empty; // "ml", "cl", "l", "g", "kg", "u"
    [JsonIgnore]
    public List<Product> Products { get; set; } = [];
}
