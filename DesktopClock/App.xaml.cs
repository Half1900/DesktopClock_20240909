using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using DesktopClock.Properties;
using Microsoft.Win32;

namespace DesktopClock;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static FileInfo MainFileInfo = new(Process.GetCurrentProcess().MainModule.FileName);

    /// <summary>
    /// Gets the time zone selected in settings, or local by default.
    /// </summary>
    public static TimeZoneInfo GetTimeZone() =>
        DateTimeUtil.TryFindSystemTimeZoneById(Settings.Default.TimeZone, out var timeZoneInfo) ? timeZoneInfo : TimeZoneInfo.Local;

    /// <summary>
    /// Sets the time zone to be used.
    /// </summary>
    public static void SetTimeZone(TimeZoneInfo timeZone) =>
        Settings.Default.TimeZone = timeZone.Id;

    /// <summary>
    /// Sets or deletes a value in the registry which enables the current executable to run on system startup.
    /// </summary>
    public static void SetRunOnStartup(bool runOnStartup)
    {
        static string GetSha256Hash(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            using var sha = new System.Security.Cryptography.SHA256Managed();
            var textData = System.Text.Encoding.UTF8.GetBytes(text);
            var hash = sha.ComputeHash(textData);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        var keyName = GetSha256Hash(MainFileInfo.FullName);
        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);

        if (runOnStartup)
            key?.SetValue(keyName, MainFileInfo.FullName); // Use the path as the name so we can handle multiple exes, but hash it or Windows won't like it.
        else
            key?.DeleteValue(keyName, false);
    }
    public static void SetSelfStart(bool b)
    {
        //MessageBox.Show(""+b);
        try
        {
            // 检查是否需要管理员权限  
            if (b)
            {
                // 设置开机自启动    
                string StartupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);
                string dir = Directory.GetCurrentDirectory();
                string exeDir = dir + @"\DesktopClock.lnk";
                //MessageBox.Show(StartupPath);
                //MessageBox.Show(exeDir);
                File.Copy(exeDir, StartupPath + @"\DesktopClock.lnk", true);

            }
            else
            {
                // 取消开机自启动    
                string StartupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Startup);
                try
                {
                    System.IO.File.Delete(StartupPath + @"\DesktopClock.lnk");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("无法删除文件: " + ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}