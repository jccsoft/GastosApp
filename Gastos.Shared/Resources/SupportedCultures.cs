using System.Collections.Immutable;

namespace Gastos.Shared.Resources;

public static class SupportedCultures
{
    public static readonly ImmutableDictionary<string, string> Cultures = ImmutableDictionary.CreateRange(
    [
        new KeyValuePair<string, string>("es-ES", "Español"),
        new KeyValuePair<string, string>("en-US", "English")
    ]);

    public static string DefaultCultureKey => "es-ES";
}
