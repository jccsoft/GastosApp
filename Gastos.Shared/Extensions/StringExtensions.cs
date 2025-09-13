namespace Gastos.Shared.Extensions;

public static class StringExtensions
{
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
