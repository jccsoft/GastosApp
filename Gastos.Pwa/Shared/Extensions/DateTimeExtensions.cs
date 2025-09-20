namespace Gastos.Pwa.Shared.Extensions;

public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime date)
    {
        return date.Date.AddDays(-(int)date.DayOfWeek + 1);
    }
    public static DateTime EndOfWeek(this DateTime date)
    {
        return date.Date.AddDays(7 - (int)date.DayOfWeek).AddHours(23).AddMinutes(59).AddSeconds(59);
    }

    public static DateTime StartOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, 1, 0, 0, 0, date.Kind);
    }
    public static DateTime EndOfMonth(this DateTime date)
    {
        return new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month), 23, 59, 59, date.Kind);
    }
    public static DateTime? ToLocalTime(this DateTimeOffset? date)
    {
        if (date is null) return null;

        return ((DateTimeOffset)date).LocalDateTime;
    }

    public static DateTimeOffset? ToUniversalTime(this DateTime? date)
    {
        if (date is null) return null;

        return ((DateTime)date).ToUniversalTime();
    }

    public static string ToLocalTimeLong(this DateTime? date)
    {
        if (date is null) return "-";

        return ((DateTime)date).ToLocalTimeLong();
    }

    public static string ToLocalTimeLong(this DateTime date)
    {
        string dateFormat = "dd/MMM/yy HH:mm:ss";

        return date.ToLocalTimeString(dateFormat);
    }

    public static string ToLocalTimeShort(this DateTimeOffset date)
    {
        return date.DateTime.ToLocalTimeShort();
    }
    public static string ToLocalTimeShort(this DateTime? date)
    {
        if (date is null) return "-";

        return ((DateTime)date).ToLocalTimeShort();
    }
    public static string ToLocalTimeShort(this DateTime date)
    {
        string dateFormat = "dd/MMM/yy";

        return date.ToLocalTimeString(dateFormat);
    }

    public static string ToLocalTimeString(this DateTime date, string dateFormat)
    {
        return date.ToLocalTime().ToString(dateFormat);
    }
}
