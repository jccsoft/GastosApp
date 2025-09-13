using System.Globalization;
using System.Resources;

namespace Gastos.Shared.Resources;

public class LocalizationService()
{
    private readonly ResourceManager _rm = new(
        typeof(Resource).FullName!,
        typeof(Resource).Assembly);

    public string Get(string key) => _rm.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    public string Get(ResourceStrings key) => _rm.GetString(key.ToString(), CultureInfo.CurrentUICulture) ?? key.ToString();

}
