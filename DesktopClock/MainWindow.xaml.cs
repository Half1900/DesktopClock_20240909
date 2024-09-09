using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DesktopClock.Properties;
using Humanizer;
using WpfWindowPlacement;
using Clipboard = System.Windows.Forms.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace DesktopClock;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
[ObservableObject]
public partial class MainWindow : Window
{
    private readonly SystemClockTimer _systemClockTimer;
    private TimeZoneInfo _timeZone;
    NotifyIcon notifyIcon = new NotifyIcon();
    private bool notify = false;

    public MainWindow()
    {
        Process[] processes = Process.GetProcesses();     //获得本机所有应用进程
        int currentCount = 0;                              //记录程序打开次数
        foreach (Process item in processes)                //循环本机所有应用进程名字
        {
            if (item.ProcessName == Process.GetCurrentProcess().ProcessName) //判断进程名字和本程序进程名字是否一致
            {
                currentCount += 1;
            }
        }
        if (currentCount > 1)     //本程序进程大于2就退出
        {
            Environment.Exit(1);
            return;
        }

        InitializeComponent();
        InitialTray();
        this.Show();
        notifyIcon.Visible = true; // 显示托盘图标
        DataContext = this;

        _timeZone = App.GetTimeZone();

        Settings.Default.PropertyChanged += Settings_PropertyChanged;

        _systemClockTimer = new();
        _systemClockTimer.SecondChanged += SystemClockTimer_SecondChanged;
        _systemClockTimer.Start();
    }
    public void InitialTray()
    {
        Visibility = Visibility.Visible;
        notifyIcon = new NotifyIcon();
        notifyIcon.Text = "桌面时间显示";
        notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
        
        // 添加右键菜单
        var contextMenu = new ContextMenu();

        var showItem = new MenuItem("显示");
        showItem.Click += Show_Click; 
        
        var hideItem = new MenuItem("隐藏");
        hideItem.Click += Hide_Click; 
       
        var exit = new MenuItem("退出");
        exit.Click += Exit_Click;




        contextMenu.MenuItems.Add(showItem);
        contextMenu.MenuItems.Add(hideItem);
        contextMenu.MenuItems.Add(exit);
        notifyIcon.ContextMenu = contextMenu;

        notify = false;
    }
    void Exit_Click(object sender,EventArgs e)
    {
        notifyIcon.Visible = false;
        this.Close();
    }
    void Show_Click(object sender, EventArgs e)
    {
        this.Show(); // 显示窗口
        this.WindowState = WindowState.Normal; // 确保窗口状态为正常
        this.Activate(); // 激活窗口
    }
    void Hide_Click(object sender, EventArgs e)
    {
        this.Hide();
        this.WindowState = WindowState.Minimized;
    }
    /// <summary>
    /// The current date and time in the selected time zone.
    /// </summary>
    private DateTimeOffset CurrentTimeInSelectedTimeZone => TimeZoneInfo.ConvertTime(DateTimeOffset.Now, _timeZone);

    /// <summary>
    /// Should the clock be a countdown?
    /// </summary>
    private bool IsCountdown => Settings.Default.CountdownTo > DateTimeOffset.MinValue;

    /// <summary>
    /// The current date and time in the selected time zone or countdown as a formatted string.
    /// </summary>
    public string CurrentTimeOrCountdownString =>
        IsCountdown ?
        Settings.Default.CountdownTo.Humanize(CurrentTimeInSelectedTimeZone) :
        CurrentTimeInSelectedTimeZone.ToString(Settings.Default.Format);

    [RelayCommand]
    public void CopyToClipboard() =>
        Clipboard.SetText(TimeTextBlock.Text);

    /// <summary>
    /// Sets app theme to parameter's value.
    /// </summary>
    [RelayCommand]
    public void SetTheme(Theme theme) => Settings.Default.Theme = theme;

    /// <summary>
    /// Sets format string in settings to parameter's string.
    /// </summary>
    [RelayCommand]
    public void SetFormat(string format) => Settings.Default.Format = format;

    /// <summary>
    /// Sets time zone ID in settings to parameter's time zone ID.
    /// </summary>
    [RelayCommand]
    public void SetTimeZone(TimeZoneInfo tzi) => App.SetTimeZone(tzi);

    /// <summary>
    /// Creates a new clock.
    /// </summary>
    [RelayCommand]
    public void NewClock()
    {
        var result = MessageBox.Show(this,
            $"该操作将复制当前的可执行文件，并使用新设置启动它。\n\n" +
            $"是否继续？",
            Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

        if (result != MessageBoxResult.OK)
            return;

        var exeInfo = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
        var newExePath = Path.Combine(exeInfo.DirectoryName, Guid.NewGuid().ToString() + exeInfo.Name);
        File.Copy(exeInfo.FullName, newExePath);
        Process.Start(newExePath);
    }

    /// <summary>
    /// Explains how to enable countdown mode.
    /// </summary>
    [RelayCommand]
    public void CountdownTo()
    {
        var result = MessageBox.Show(this,
            $"In advanced settings: change {nameof(Settings.Default.CountdownTo)}, then save.\n" +
            "Go back by replacing it with \"0001-01-01T00:00:00+00:00\".\n\n" +
            "Open advanced settings now?",
            Title, MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);

        if (result == MessageBoxResult.OK)
            OpenSettings();
    }

    /// <summary>
    /// Opens the settings file in Notepad.
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        Settings.Default.Save();

        // Re-create the settings file if it got deleted.
        if (!File.Exists(Settings.FilePath))
            Settings.Default.Save();

        // Open settings file in notepad.
        try
        {
            Process.Start("notepad", Settings.FilePath);
        }
        catch (Exception ex)
        {
            // Lazy scammers on the Microsoft Store may reupload without realizing it's sandboxed, which makes it unable to start the Notepad process.
            MessageBox.Show(this,
                "Couldn't open settings file.\n\n" +
                "This app may have be reuploaded without permission. If you paid for it, ask for a refund and download it for free from the original source: https://github.com/danielchalmers/DesktopClock.\n\n" +
                $"If it still doesn't work, report it as an issue at that link with details on what happened and include this error: \"{ex.Message}\"");
        }
    }

    [RelayCommand]
    public void MiniWindow(){
         // 最小化窗口并隐藏
        this.Hide();
        notifyIcon.Visible = true; // 显示托盘图标
    }
    /// <summary>
    /// Checks for updates.
    /// </summary>
    [RelayCommand]
    public void CheckForUpdates()
    {
        Process.Start("https://github.com/danielchalmers/DesktopClock/releases");
    }

    /// <summary>
    /// Exits the program.
    /// </summary>
    [RelayCommand]
    public void Exit() => Close();

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
        }
    }

    private void SystemClockTimer_SecondChanged(object sender, EventArgs e)
    {
        UpdateTimeString();
    }

    private void UpdateTimeString() => OnPropertyChanged(nameof(CurrentTimeOrCountdownString));

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
            // 确保窗口边界不超过可视桌面
            var workingArea = SystemParameters.WorkArea;
            Left = Math.Max(workingArea.Left, Math.Min(workingArea.Right - Width, Left));
            Top = Math.Max(workingArea.Top, Math.Min(workingArea.Bottom - Height, Top));
        }
    }

    private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        CopyToClipboard();
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Scale size based on scroll amount, with one notch on a default PC mouse being a change of 15%.
            var steps = e.Delta / (double)Mouse.MouseWheelDeltaForOneLine;
            var change = Settings.Default.Height * steps * 0.15;
            Settings.Default.Height = (int)Math.Min(Math.Max(Settings.Default.Height + change, 16), 160);
        }
    }

    private void Window_SourceInitialized(object sender, EventArgs e)
    {
        WindowPlacementFunctions.SetPlacement(this, Settings.Default.Placement);
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        Settings.Default.Placement = WindowPlacementFunctions.GetPlacement(this);

        Settings.Default.SaveIfNotModifiedExternally();

        App.SetRunOnStartup(Settings.Default.RunOnStartup);

        Settings.Default.Dispose();
    }
}