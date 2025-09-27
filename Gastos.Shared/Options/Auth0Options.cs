namespace Gastos.Shared.Options;

public sealed class Auth0Options
{
    public const string ConfigurationSection = "Auth0";

    public string Domain { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
