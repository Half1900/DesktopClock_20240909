using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopClock;

public static class DateTimeUtil
{
    /// <summary>
    /// A collection of DateTime formatting strings.
    /// </summary>
    public static IReadOnlyList<string> DateTimeFormats { get; } = new[]
    {
        
        "HH:mm",
        "mm:ss",
        "HH:mm:ss",
        "MM-dd HH:mm",
        "MM-dd HH:mm:ss",
        "MM-dd",
        "MM-dd ddd",
        "MM-dd dddd",
        "MM-dd ddd HH:mm",
        "MM-dd dddd HH:mm",
        "MM-dd ddd HH:mm:ss",
        "MM-dd dddd HH:mm:ss",
        "ddd",
        "dddd",
        "ddd HH:mm",
        "dddd HH:mm",
        "ddd HH:mm:ss",
        "dddd HH:mm:ss",
        "yy-MM-dd",
        "yy-MM-dd ddd",
        "yy-MM-dd dddd",
        "yy-MM-dd HH:mm",
        "yy-MM-dd HH:mm:ss",
        "yy-MM-dd ddd HH:mm",
        "yy-MM-dd dddd HH:mm",
        "yy-MM-dd ddd HH:mm:ss",
        "yy-MM-dd dddd HH:mm:ss",
        "yyyy-MM-dd",
        "yyyy-MM-dd ddd",       
        "yyyy-MM-dd dddd",        
        "yyyy-MM-dd HH:mm",        
        "yyyy-MM-dd HH:mm:ss",        
        "yyyy-MM-dd ddd HH:mm",       
        "yyyy-MM-dd dddd HH:mm",       
        "yyyy-MM-dd ddd HH:mm:ss",       
        "yyyy-MM-dd dddd HH:mm:ss"
    };

    /// <summary>
    /// Common date time formatting strings and an example string for each.
    /// </summary>
    public static IReadOnlyDictionary<string, string> DateTimeFormatsAndExamples { get; } =
        DateTimeFormats.ToDictionary(f => f, DateTimeOffset.Now.ToString);

    public static IReadOnlyCollection<TimeZoneInfo> TimeZones { get; } = TimeZoneInfo.GetSystemTimeZones();

    public static bool TryGetTimeZoneById(string timeZoneId, out TimeZoneInfo timeZoneInfo)
    {
        try
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            timeZoneInfo = null;
            return false;
        }
    }
}