namespace Gastos.Api.Shared.Options;

public sealed class GastosApiOptions
{
    public const string ConfigurationSection = "GastosApi";

    public required string BaseUrl { get; set; }
    public required bool LockUnauthenticated { get; set; }
}
