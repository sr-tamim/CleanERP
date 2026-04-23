using System.ComponentModel;
using System.Reflection;

namespace CleanERP.Shared.Extensions;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }

    public static T ToEnum<T>(this string value) where T : struct, Enum
    {
        return Enum.TryParse<T>(value, true, out var result) ? result : default;
    }
}

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }

    public static bool IsNullOrWhiteSpace(this string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    public static string ToTitleCase(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }

    public static string Truncate(this string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            return value;

        return value[..maxLength] + "...";
    }

    public static string ToSafeFileName(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var invalidChars = Path.GetInvalidFileNameChars();
        var safeValue = new string(value.Where(c => !invalidChars.Contains(c)).ToArray());
        return safeValue.Replace(" ", "_");
    }
}

public static class DateTimeExtensions
{
    public static string ToFriendlyString(this DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        return timeSpan.TotalDays switch
        {
            < 1 when timeSpan.TotalHours < 1 => $"{(int)timeSpan.TotalMinutes} minutes ago",
            < 1 => $"{(int)timeSpan.TotalHours} hours ago",
            < 7 => $"{(int)timeSpan.TotalDays} days ago",
            < 30 => $"{(int)(timeSpan.TotalDays / 7)} weeks ago",
            < 365 => $"{(int)(timeSpan.TotalDays / 30)} months ago",
            _ => $"{(int)(timeSpan.TotalDays / 365)} years ago"
        };
    }

    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.Today;
    }

    public static bool IsThisWeek(this DateTime dateTime)
    {
        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        return dateTime >= startOfWeek && dateTime < endOfWeek;
    }

    public static bool IsThisMonth(this DateTime dateTime)
    {
        return dateTime.Year == DateTime.Today.Year && dateTime.Month == DateTime.Today.Month;
    }
}

public static class DecimalExtensions
{
    public static string ToCurrency(this decimal value, string? currencySymbol = "$")
    {
        return $"{currencySymbol}{value:N2}";
    }

    public static decimal ToPercentage(this decimal value, int decimalPlaces = 2)
    {
        return Math.Round(value * 100, decimalPlaces);
    }

    public static bool IsPositive(this decimal value)
    {
        return value > 0;
    }

    public static bool IsNegative(this decimal value)
    {
        return value < 0;
    }
}
