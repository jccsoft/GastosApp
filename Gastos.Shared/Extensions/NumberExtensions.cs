using System.Globalization;

namespace Gastos.Shared.Extensions;

public static class NumberExtensions
{
    public static string ToStringAmount(this decimal value)
    {
        return value.ToString("C2");
    }

    public static string ToStringQuantity(this decimal value)
    {
        var decimalSep = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var groupSep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
        return value % 1 == 0 ? value.ToString("F0") : value.ToString("F3").Replace(groupSep, decimalSep).TrimEnd('0');
    }
}
