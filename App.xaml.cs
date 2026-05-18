using Calender.Data;
using Calender.Services;
using Microsoft.UI.Xaml;

namespace Calender;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
        InitializeDatabase();

        var cfg = new SettingsService().Load();

        ElementSoundPlayer.State            = cfg.SoundEnabled
            ? ElementSoundPlayerState.On
            : ElementSoundPlayerState.Off;
        ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();

        // Apply saved theme to the root element after the window is created
        var theme = new SettingsService().Load().Theme;
        if (MainWindow.Content is FrameworkElement root)
            root.RequestedTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark"  => ElementTheme.Dark,
                _       => ElementTheme.Default,
            };
    }

    // +--------------------------------------------------+
    // |                  DATABASE INIT                   |
    // +--------------------------------------------------+

    private static void InitializeDatabase()
    {
        using var ctx = new AppDbContext();
        ctx.Database.EnsureCreated();
    }
}
