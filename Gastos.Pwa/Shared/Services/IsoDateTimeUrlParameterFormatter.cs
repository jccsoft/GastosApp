using Refit;
using System.Reflection;

namespace Gastos.Pwa.Shared.Services;

public class IsoDateTimeUrlParameterFormatter : IUrlParameterFormatter
{
    public string? Format(object? value, ICustomAttributeProvider attributeProvider, Type type)
    {
        if (value is DateTime dt)
            return dt.ToString("o"); // ISO 8601

        if (value is DateTimeOffset dto)
            return dto.ToString("o"); // ISO 8601

        return value?.ToString();
    }
}