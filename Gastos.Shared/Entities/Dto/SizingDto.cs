namespace Gastos.Shared.Entities.Dto;

public sealed class SizingDto
{
    public int Id { get; set; } // 1=ml, 2=cl, 3=l, 4=g, 5=kg, 6=u
    public string Name { get; set; } = string.Empty; // "ml", "cl", "l", "g", "kg", "u"
    public List<ProductDto> Products { get; set; } = [];
}
