using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;

namespace Calender.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    // +--------------------------------------------------+
    // |                    FIELDS                        |
    // +--------------------------------------------------+

    private readonly SettingsService _settings = new();

    // +--------------------------------------------------+
    // |                   PROPERTIES                     |
    // +--------------------------------------------------+

    /// <summary>0 = System default, 1 = Light, 2 = Dark.</summary>
    [ObservableProperty] private int  _selectedThemeIndex;
    [ObservableProperty] private bool _soundEnabled;
    [ObservableProperty] private bool _widgetAlwaysOnTop;
    [ObservableProperty] private bool _widgetSizeLarge;

    public string DataFolder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Calender");

    public string AppVersion => "0.6.0";

    // +--------------------------------------------------+
    // |                  CONSTRUCTION                    |
    // +--------------------------------------------------+

    public SettingsViewModel()
    {
        var cfg = _settings.Load();
        _selectedThemeIndex = cfg.Theme switch { "Light" => 1, "Dark" => 2, _ => 0 };
        _soundEnabled       = cfg.SoundEnabled;
        _widgetAlwaysOnTop  = cfg.WidgetAlwaysOnTop;
        _widgetSizeLarge    = cfg.WidgetSize != "Small";
    }

    // +--------------------------------------------------+
    // |               PROPERTY CALLBACKS                 |
    // +--------------------------------------------------+

    partial void OnSelectedThemeIndexChanged(int value)
    {
        var theme = value switch
        {
            1 => ElementTheme.Light,
            2 => ElementTheme.Dark,
            _ => ElementTheme.Default,
        };

        if (App.MainWindow?.Content is FrameworkElement root)
            root.RequestedTheme = theme;

        Persist(cfg => cfg.Theme = value switch { 1 => "Light", 2 => "Dark", _ => "System" });
    }

    partial void OnSoundEnabledChanged(bool value)
    {
        ElementSoundPlayer.State = value
            ? ElementSoundPlayerState.On
            : ElementSoundPlayerState.Off;

        Persist(cfg => cfg.SoundEnabled = value);
    }

    partial void OnWidgetAlwaysOnTopChanged(bool value)
        => Persist(cfg => cfg.WidgetAlwaysOnTop = value);

    partial void OnWidgetSizeLargeChanged(bool value)
        => Persist(cfg => cfg.WidgetSize = value ? "Large" : "Small");

    // +--------------------------------------------------+
    // |                   COMMANDS                       |
    // +--------------------------------------------------+

    [RelayCommand]
    private void OpenDataFolder()
    {
        try { System.Diagnostics.Process.Start("explorer.exe", DataFolder); }
        catch { /* best-effort */ }
    }

    // +--------------------------------------------------+
    // |                    HELPERS                       |
    // +--------------------------------------------------+

    private void Persist(Action<AppSettings> mutate)
    {
        var cfg = _settings.Load();
        mutate(cfg);
        _settings.Save(cfg);
    }
}
