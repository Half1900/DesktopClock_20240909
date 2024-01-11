using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClock.Properties;
using H.NotifyIcon;
using Humanizer;

namespace DesktopClock;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ObservableObject]
public partial class MainWindow : Window
{
    private bool _hasInitiallyChangedSize;
    private readonly SystemClockTimer _systemClockTimer;
    private TaskbarIcon _trayIcon;
    private TimeZoneInfo _timeZone;
    //NotifyIcon notifyIcon;
    bool tag = true;
    /// <summary>
    /// The date and time to countdown to, or null if regular clock is desired.
    /// </summary>
    [ObservableProperty]
    private DateTimeOffset? _countdownTo;

    /// <summary>
    /// The current date and time in the selected time zone or countdown as a formatted string.
    /// </summary>
    [ObservableProperty]
    private string _currentTimeOrCountdownString;

    public static readonly double MaxSizeLog = 5.0;
    public static readonly double MinSizeLog = 2.0;

    public MainWindow()
    {
        string applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;// 获取当前正在运行的应用程序进程列表
                                                                                                  //MessageBox.Show(applicationName);                                                                                                      
        Process[] processes = Process.GetProcesses(); // 获取当前正在运行的应用程序进程列表             
        foreach (Process process in processes) // 遍历进程列表，查找与当前应用程序名称相同的进程  
        {
            if (process.ProcessName == applicationName && process.Id != Process.GetCurrentProcess().Id)
            {
                process.CloseMainWindow();    // 关闭旧进程
                                              //MessageBox.Show("旧的应用程序已关闭", "提示", MessageBoxButton.OK, MessageBoxImage.Information); // 显示关闭成功消息框
            }
        }
        InitializeComponent();
        DataContext = this;
        //IconTray();
        //ContextMenu1();
        _timeZone = App.GetTimeZone();
        UpdateCountdownEnabled();

        Settings.Default.PropertyChanged += (s, e) => Dispatcher.Invoke(() => Settings_PropertyChanged(s, e));

        _systemClockTimer = new();
        _systemClockTimer.SecondChanged += SystemClockTimer_SecondChanged;
        _systemClockTimer.Start();

        ContextMenu = Resources["MainContextMenu"] as System.Windows.Controls.ContextMenu;

        ConfigureTrayIcon(Settings.Default.ShowInTaskbar, true);
    }

    /// <summary>
    /// Copies the current time string to the clipboard.
    /// </summary>
    [RelayCommand]
    public void CopyToClipboard() => System.Windows.Forms.Clipboard.SetText(CurrentTimeOrCountdownString);

    /// <summary>
    /// Minimizes the window.
    /// </summary>
    [RelayCommand]
    public void HideForNow( bool b)
    {
        if(b)
        {
            if (!Settings.Default.TipsShown.HasFlag(TeachingTips.HideForNow))
            {
                //System.Windows.MessageBox.Show(this, "时钟将最小化，并且可以从任务栏或系统托盘再次打开（如果启用）。",Title, MessageBoxButton.OK, MessageBoxImage.Information);

                Settings.Default.TipsShown |= TeachingTips.HideForNow;
            }
            tag = false;
            this.Visibility = Visibility.Collapsed;
            //ConfigureTrayIcon(!Settings.Default.ShowInTaskbar, true);
            //WindowState = WindowState.Minimized;
        }
        else
        {
            tag = true;
            this.Visibility = Visibility.Visible;
        }
    }
    /*private void IconTray()
    {
        string path = System.IO.Path.GetFullPath(@"image/DesktopClock.ico");
        System.Drawing.Icon nIcon = new System.Drawing.Icon(path);//程序图标

        if (File.Exists(path))
        {
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "桌面时钟";
            this.notifyIcon.Text = "桌面时钟";
            this.notifyIcon.Icon = nIcon; // System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath) ;
            this.notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseClicks;
        }
    }
    private void NotifyIcon_MouseClicks(object sender, System.Windows.Forms.MouseEventArgs e)
    {
        tag = !tag;
        Settings.Default.ShowOrHide = !Settings.Default.ShowOrHide;
        System.Windows.Forms.MessageBox.Show(Settings.Default.ShowOrHide + "");
        System.Windows.Forms.MessageBox.Show(tag+"");
        if (tag)
        {
            this.Visibility = Visibility.Visible;
        }
        else
        {
            this.Visibility = Visibility.Collapsed;
        }
    }
    private new void ContextMenu1()
    {
        ContextMenuStrip cms = new ContextMenuStrip();

        //关联 NotifyIcon 和 ContextMenuStrip
        notifyIcon.ContextMenuStrip = cms;

        System.Windows.Forms.ToolStripMenuItem exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        exitMenuItem.Text = "退出";
        exitMenuItem.Click += new EventHandler(exitMenuItem_Click);

        System.Windows.Forms.ToolStripMenuItem hideMenumItem = new System.Windows.Forms.ToolStripMenuItem();
        hideMenumItem.Text = "隐藏";
        hideMenumItem.Click += new EventHandler(hideMenumItem_Click);

        System.Windows.Forms.ToolStripMenuItem showMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        showMenuItem.Text = "显示";
        showMenuItem.Click += new EventHandler(showMenuItem_Click);

        cms.Items.Add(showMenuItem);
        cms.Items.Add(hideMenumItem);
        cms.Items.Add(exitMenuItem);
    }
    private void exitMenuItem_Click(object sender, EventArgs e)
    {
        Environment.Exit(0);
        //Close();
    }

    private void hideMenumItem_Click(object sender, EventArgs e)
    {
        //tag = false;
        this.Visibility = Visibility.Collapsed;
    }

    private void showMenuItem_Click(object sender, EventArgs e)
    {
        //tag = true;
        this.Visibility = Visibility.Visible;
    }
    */
    /// <summary>
    /// Sets app's theme to given value.
    /// </summary>
    [RelayCommand]
    public void SetTheme(Theme theme) => Settings.Default.Theme = theme;

    /// <summary>
    /// Sets format string in settings to given string.
    /// </summary>
    [RelayCommand]
    public void SetFormat(string format) => Settings.Default.Format = format;

    /// <summary>
    /// Explains how to write a format, then asks user if they want to view a website and Advanced settings to do so.
    /// </summary>
    [RelayCommand]
    public void FormatWizard()
    {
        var result = System.Windows.MessageBox.Show(this,
            $"在高级设置中: 编辑 \"{nameof(Settings.Default.Format)}\" 使用特殊的“自定义日期和时间格式字符串”，然后保存" +
            "\n\n现在打开高级设置？",
            Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

        if (result != MessageBoxResult.OK)
            return;

        //Process.Start("https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings");
        OpenSettings();
    }

    /// <summary>
    /// Sets time zone ID in settings to given time zone ID.
    /// </summary>
    [RelayCommand]
    public void SetTimeZone(TimeZoneInfo tzi) => App.SetTimeZone(tzi);

    /// <summary>
    /// Creates a new clock executable and starts it.
    /// </summary>
    [RelayCommand]
    public void NewClock()
    {
        if (!Settings.Default.TipsShown.HasFlag(TeachingTips.NewClock))
        {
            var result = System.Windows.MessageBox.Show(this,
                "这将复制可执行文件并使用新设置启动它。\n\n" +
                "是否继续？",
                Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

            if (result != MessageBoxResult.OK)
                return;

            Settings.Default.TipsShown |= TeachingTips.NewClock;
        }

        var newExePath = Path.Combine(App.MainFileInfo.DirectoryName, App.MainFileInfo.GetFileAtNextIndex().Name);

        // Copy and start the new clock.
        File.Copy(App.MainFileInfo.FullName, newExePath);
        Process.Start(newExePath);
    }

    /// <summary>
    /// Explains how to enable countdown mode, then asks user if they want to view Advanced settings to do so.
    /// </summary>
    [RelayCommand]
    public void CountdownWizard()
    {
       /* var result = System.Windows.MessageBox.Show(this,
            $"在高级设置中: 更改 \"{nameof(Settings.Default.CountdownTo)}\" 格式为 \"{default(DateTime)}\", 然后保存。" +
            "\n\n立即打开高级设置？",
            Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

        if (result != MessageBoxResult.OK)
            return;

        OpenSettings();*/
    }

    /// <summary>
    /// Opens the settings file in Notepad.
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        // Save first if we can so it's up-to-date.
        if (Settings.CanBeSaved)
            Settings.Default.Save();

        // If it doesn't even exist then it's probably somewhere that requires special access and we shouldn't even be at this point.
        if (!Settings.Exists)
            return;

        // Teach user how it works.
        if (!Settings.Default.TipsShown.HasFlag(TeachingTips.AdvancedSettings))
        {
            System.Windows.MessageBox.Show(this,
                "设置以JSON格式存储，并将在记事本中打开。只需保存文件，即可看到您的更改显示在时钟上。若要重新开始，请删除“.settings”文件。",
                Title, MessageBoxButton.OK, MessageBoxImage.Information);

            Settings.Default.TipsShown |= TeachingTips.AdvancedSettings;
        }

        // Open settings file in notepad.
        try
        {
            Process.Start("notepad", Settings.FilePath);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex);
            // Lazy scammers on the Microsoft Store may reupload without realizing it gets sandboxed, making it unable to start the Notepad process (#1, #12).
            /*System.Windows.MessageBox.Show(this,
                "无法打开设置文件。\n\n" +
                "此应用程序可能在未经许可的情况下被重新加载。如果您付费，请要求退款并从原始来源免费下载: https://github.com/danielchalmers/DesktopClock.\n\n" +
                $"如果仍然不起作用，请在该链接上创建一个新的Issue，其中包含所发生事情的详细信息，并包含此错误: \"{ex.Message}\"",
                Title, MessageBoxButton.OK, MessageBoxImage.Error);*/
        }
    }

    /// <summary>
    /// Opens the GitHub Releases page.
    /// </summary>
    [RelayCommand]
    public void CheckForUpdates()
    {
        if (!Settings.Default.TipsShown.HasFlag(TeachingTips.CheckForUpdates))
        {
            var result = System.Windows.MessageBox.Show(this,
                "这将带您访问一个网站以查看最新版本。\n\n" +
                "是否继续？",
                Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

            if (result != MessageBoxResult.OK)
                return;

            Settings.Default.TipsShown |= TeachingTips.CheckForUpdates;
        }

        //Process.Start("https://github.com/danielchalmers/DesktopClock/releases");
    }

    /// <summary>
    /// Exits the program.
    /// </summary>
    [RelayCommand]
    public void Exit()
    {
        Close();
    }

    private void ConfigureTrayIcon(bool showIcon, bool firstLaunch)
    {
        if (showIcon)
        {
            if (_trayIcon == null)
            {
                _trayIcon = Resources["TrayIcon"] as TaskbarIcon;
                _trayIcon.ContextMenu = Resources["MainContextMenu"] as System.Windows.Controls.ContextMenu;
                _trayIcon.ContextMenu.DataContext = this;
                _trayIcon.ForceCreate(enablesEfficiencyMode: false);
                _trayIcon.TrayLeftMouseDoubleClick += NotifyIcon_MouseClick;
            }

           /* if (!firstLaunch)
            {
                _trayIcon.ShowNotification("从任务栏隐藏", "图标已移动到托盘");
            }*/
               
        }
        else
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
        }
    }

    private void NotifyIcon_MouseClick(object sender, RoutedEventArgs e) {

        tag = !tag;
        //System.Windows.Forms.MessageBox.Show(tag+"1");
        Settings.Default.ShowOrHide = !Settings.Default.ShowOrHide;
        //System.Windows.Forms.MessageBox.Show(Settings.Default.ShowOrHide + "");
        if (tag)
        {
            this.Visibility = Visibility.Visible;
            //WindowState = WindowState.Normal;
            Topmost = true;
        }
        else
        {
            this.Visibility = Visibility.Collapsed;
            //WindowState = WindowState.Minimized;
        }
    }

    private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Settings.Default.TimeZone):
                _timeZone = App.GetTimeZone();
                UpdateTimeString();
                break;

            case nameof(Settings.Default.Format):
                UpdateTimeString();
                break;

            case nameof(Settings.Default.ShowInTaskbar):
                ConfigureTrayIcon(!Settings.Default.ShowInTaskbar, false);
                break;

            case nameof(Settings.Default.CountdownTo):
                UpdateCountdownEnabled();
                break;
            case nameof(Settings.Default.RunOnStartup):
                App.SetSelfStart(Settings.Default.RunOnStartup);
                break;
            case nameof(Settings.Default.ShowOrHide):                
                HideForNow(Settings.Default.ShowOrHide);
                break;
        }
    }

    private void SystemClockTimer_SecondChanged(object sender, EventArgs e)
    {
        UpdateTimeString();
    }

    private void UpdateCountdownEnabled()
    {
        if (Settings.Default.CountdownTo == null || Settings.Default.CountdownTo == default(DateTime))
        {
            CountdownTo = null;
            return;
        }

        CountdownTo = new DateTimeOffset(Settings.Default.CountdownTo.Value, _timeZone.BaseUtcOffset);
    }

    private void UpdateTimeString()
    {
        string GetTimeString()
        {
            var timeInSelectedZone = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, _timeZone);

            if (CountdownTo == null)
            {
                return Tokenizer.FormatWithTokenizerOrFallBack(timeInSelectedZone, Settings.Default.Format, CultureInfo.DefaultThreadCurrentCulture);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.CountdownFormat))
                    return CountdownTo.Humanize(timeInSelectedZone);

                return Tokenizer.FormatWithTokenizerOrFallBack(Settings.Default.CountdownTo - timeInSelectedZone, Settings.Default.CountdownFormat, CultureInfo.DefaultThreadCurrentCulture);
            }
        }

        CurrentTimeOrCountdownString = GetTimeString();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left && Settings.Default.DragToMove)
        {
            DragMove();
        }
    }

    private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        CopyToClipboard();
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        /*        if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    // Amount of scroll that occurred and whether it was positive or negative.
                    var steps = e.Delta / (double)Mouse.MouseWheelDeltaForOneLine;

                    // Convert the height, adjust it, then convert back in the same way as the slider.
                    var newHeightLog = Math.Log(Settings.Default.Height) + (steps * 0.15);
                    var newHeightLogClamped = Math.Min(Math.Max(newHeightLog, MinSizeLog), MaxSizeLog);
                    var exp = Math.Exp(newHeightLogClamped);

                    // Apply the new height as an integer to make it easier for the user.
                    Settings.Default.Height = (int)exp;
                }*/

        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Scale size based on scroll amount, with one notch on a default PC mouse being a change of 15%.
            var steps = e.Delta / (double)Mouse.MouseWheelDeltaForOneLine;
            var change = Settings.Default.Height * steps * 0.15;
            Settings.Default.Height = (int)Math.Min(Math.Max(Settings.Default.Height + change, 16), 160);
        }
    }

    private void Window_ContentRendered(object sender, EventArgs e)
    {
        SizeChanged += Window_SizeChanged;

        if (!Settings.CanBeSaved)
        {
            System.Windows.MessageBox.Show(this,
                "Settings can't be saved because of an access error.\n\n" +
                $"Make sure {Title} is in a folder that doesn't require admin privileges, " +
                "and that you got it from the original source: https://github.com/danielchalmers/DesktopClock.\n\n" +
                "If the problem still persists, feel free to create a new Issue at the above link with as many details as possible.",
                Title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        // Stop the file watcher before saving.
        Settings.Default.Dispose();

        if (Settings.CanBeSaved)
            Settings.Default.Save();

        //App.SetRunOnStartup(Settings.Default.RunOnStartup);
        App.SetSelfStart(Settings.Default.RunOnStartup);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_hasInitiallyChangedSize && e.WidthChanged && Settings.Default.RightAligned)
        {
            var previousRight = Left + e.PreviousSize.Width;
            Left = previousRight - ActualWidth;
        }

        // Use this to ignore the change when the window is loaded at the beginning.
        _hasInitiallyChangedSize = true;
    }
}