using Calender.Data;
using Microsoft.UI.Xaml;

namespace Calender;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public App()
    {
        this.InitializeComponent();
        InitializeDatabase();

        // Enable built-in WinUI interaction sounds (button clicks, selection, etc.)
        ElementSoundPlayer.State = ElementSoundPlayerState.On;
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
