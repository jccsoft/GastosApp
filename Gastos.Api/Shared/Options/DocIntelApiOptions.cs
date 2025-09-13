namespace Gastos.Api.Shared.Options;

public sealed class DocIntelApiOptions
{
    public const string ConfigurationSection = "DocIntelApi";

    public required string ApiKey { get; set; }
    public required string BaseUrl { get; set; }
}
