using System.Globalization;
using System.Text;

namespace Gastos.Shared.Extensions;

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

    public static string FirstCharToUpper(this string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        Span<char> destination = stackalloc char[1];

        input.AsSpan(0, 1).ToUpperInvariant(destination);

        return $"{destination}{input.ToLower().AsSpan(1)}";
    }

    public static string RemoveAllSpaces(this string input)
    {
        string output = input?.Replace(" ", string.Empty) ?? string.Empty;

        return output;
    }
}
