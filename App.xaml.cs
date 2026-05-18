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

        // Restore sound preference from settings (widget toggle controls this at runtime)
        var soundEnabled = new SettingsService().Load().SoundEnabled;
        ElementSoundPlayer.State = soundEnabled
            ? ElementSoundPlayerState.On
            : ElementSoundPlayerState.Off;
        ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();
    }

  
    /// Ensures the SQLite database and schema exist before the UI loads.
    
    private static void InitializeDatabase()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();
    }
}
