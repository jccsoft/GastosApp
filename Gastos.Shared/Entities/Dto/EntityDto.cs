namespace Gastos.Shared.Entities.Dto;

public class EntityDto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
}
