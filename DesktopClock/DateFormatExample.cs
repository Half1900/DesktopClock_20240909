using System;
using System.Collections.Generic;
using System.Linq;

namespace DesktopClock;

public record DateFormatExample
{
    private DateFormatExample(string format, string example)
    {
        Format = format;
        Example = example;
    }

    public string Format { get; }
    public string Example { get; }

    public static DateFormatExample Tutorial => new(string.Empty, "创建自定义格式");

    /// <summary>
    /// Creates a <see cref="DateFormatExample" /> from the given format.
    /// </summary>
    public static DateFormatExample FromFormat(string format) => new(format, DateTimeOffset.Now.ToString(format));

    /// <summary>
    /// Common date time formatting strings and an example string for each.
    /// </summary>
    /// <remarks>https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings</remarks>
    public static IReadOnlyCollection<DateFormatExample> DefaultExamples { get; } = new[]
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
    "yyyy-MM-dd dddd HH:mm:ss",
    }.Select(FromFormat).Append(Tutorial).ToList();
}