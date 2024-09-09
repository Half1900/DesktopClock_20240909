using System;
using System.Collections.Generic;
using System.Windows;
using DesktopClock.Properties;
using Microsoft.Win32;

namespace DesktopClock;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // https://www.materialui.co/colors - A100, A700.
    public static IReadOnlyList<Theme> Themes { get; } = new[]
    {
        new Theme("浅色", "#F5F5F5", "#212121"),
        new Theme("深色", "#212121", "#F5F5F5"),
        new Theme("红色", "#D50000", "#FF8A80"),
        new Theme("红色1", "#D50000", "#FF8A80"),
        new Theme("粉色", "#C51162", "#FF80AB"),
        new Theme("粉色1", "#C51162", "#FF80AB"),
        new Theme("紫色", "#AA00FF", "#EA80FC"),
        new Theme("紫色1", "#AA00FF", "#EA80FC"),
        new Theme("蓝色", "#2962FF", "#82B1FF"),
        new Theme("蓝色1", "#2962FF", "#82B1FF"),
        new Theme("青色", "#00B8D4", "#84FFFF"),
        new Theme("绿色", "#00AD48", "#C9F8D4"),
        new Theme("绿色", "#00C853", "#B9F6CA"),
        new Theme("绿色1", "#00C853", "#B9F6CA"),
        new Theme("橙色", "#FF6D00", "#FFD180"),
        new Theme("橙色1", "#FF6D00", "#FFD180"),
        new("白字红底", "#FFFFFF", "#FF0000"),
        new("白字粉底", "#FFFFFF", "#FFC2BD"),
        new("白字紫底", "#FFFFFF", "#BB33FF"),
        new("白字蓝底", "#FFFFFF", "#C7DBFF"),
        new("白字青底", "#FFFFFF", "#A8FFFF"),
        new("白字绿底", "#FFFFFF", "#C9F8D4"),
        new("白字绿底1", "#FFFFFF", "#008000"),
        new("白字橙底", "#FFFFFF", "#FFEFD6"),
        new("白字棕底", "#FFFFFF", "#996633"),
        new("白字蓝灰底", "#FFFFFF", "#607D8B")
    };

    /// <summary>
    /// Gets the time zone selected in settings, or local by default.
    /// </summary>
    public static TimeZoneInfo GetTimeZone() =>
        DateTimeUtil.TryGetTimeZoneById(Settings.Default.TimeZone, out var timeZoneInfo) ? timeZoneInfo : TimeZoneInfo.Local;

    /// <summary>
    /// Selects a time zone to use.
    /// </summary>
    public static void SetTimeZone(TimeZoneInfo timeZone) =>
        Settings.Default.TimeZone = timeZone.Id;

    /// <summary>
    /// Sets a value in the registry determining whether the current executable should run on system startup.
    /// </summary>
    /// <param name="runOnStartup"></param>
    public static void SetRunOnStartup(bool runOnStartup)
    {
        var exePath = ResourceAssembly.Location;
        var keyName = GetSha256Hash(exePath);
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        if (runOnStartup)
            key?.SetValue(keyName, exePath); // Use the path as the name so we can handle multiple exes, but hash it or Windows won't like it.
        else
            key?.DeleteValue(keyName, false);
    }

    internal static string GetSha256Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using var sha = new System.Security.Cryptography.SHA256Managed();
        var textData = System.Text.Encoding.UTF8.GetBytes(text);
        var hash = sha.ComputeHash(textData);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}