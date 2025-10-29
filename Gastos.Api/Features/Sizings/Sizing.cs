namespace Gastos.Api.Features.Sizings;

public sealed class Sizing
{
    public int Id { get; set; } // 1=ml, 2=cl, 3=l, 4=g, 5=kg, 6=u
    public string Name { get; set; } = string.Empty; // "ml", "cl", "l", "g", "kg", "u"
    
    // Relación con la unidad padre
    public int? ParentId { get; set; }
    [JsonIgnore]
    public Sizing? Parent { get; set; }
    
    // Proporción respecto al padre (ej: 1000 para ml -> L)
    public decimal? Proportion { get; set; }
    
    // Relación inversa: hijos de esta unidad
    [JsonIgnore]
    public List<Sizing> Children { get; set; } = [];
    
    [JsonIgnore]
    public List<Product> Products { get; set; } = [];
}
