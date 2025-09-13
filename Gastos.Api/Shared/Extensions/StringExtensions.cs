using System.Text;

namespace Gastos.Api.Shared.Extensions;

public static class StringExtensions
{
    public static string UrlDecode(this string value)
    {
        return string.IsNullOrEmpty(value) ? value : Uri.UnescapeDataString(value);
    }

    public static string RemoveAccents(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).Normalize(NormalizationForm.FormC);
    }
}
