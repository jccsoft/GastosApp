namespace Gastos.Shared.Entities.Request;

public class StatParameters
{
    public StatType Period { get; set; }
    public DateTimeOffset? DateStartUtc { get; set; }
    public DateTimeOffset? DateEndUtc { get; set; }
}

