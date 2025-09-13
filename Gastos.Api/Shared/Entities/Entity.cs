namespace Gastos.Api.Shared.Entities;

public class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
}
