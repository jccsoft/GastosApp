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

    // función para sustituir caracteres no alfabéticos por espacios, eliminar espacios múltiples y recortar espacios al inicio y al final, y devolver la primera palabra
    public static string GetFirstWord(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sb = new StringBuilder();

        bool lastWasSpace = true;
        foreach (char c in input)
        {
            if (char.IsLetter(c))
            {
                sb.Append(c);
                lastWasSpace = false;
            }
            else
            {
                if (!lastWasSpace)
                {
                    sb.Append(' ');
                    lastWasSpace = true;
                }
            }
        }

        var result = sb.ToString().Trim();
        var firstSpaceIndex = result.IndexOf(' ');
        if (firstSpaceIndex >= 0)
        {
            return result[..firstSpaceIndex];
        }

        return result;
    }
}
