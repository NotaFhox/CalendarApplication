namespace Calender.Models;

/// <summary>
/// Persisted user preferences — stored alongside the SQLite DB in
/// %LOCALAPPDATA%\Calender\settings.json.
/// </summary>
public class AppSettings
{
    // +--------------------------------------------------+
    // |                 WIDGET POSITION                  |
    // +--------------------------------------------------+

    public int WidgetX { get; set; } = 80;
    public int WidgetY { get; set; } = 80;

    // +--------------------------------------------------+
    // |                 WIDGET BEHAVIOUR                 |
    // +--------------------------------------------------+

    /// <summary>"Small" | "Large"</summary>
    public string WidgetSize        { get; set; } = "Large";
    public bool   WidgetAlwaysOnTop { get; set; } = true;

    // +--------------------------------------------------+
    // |               APP-WIDE PREFERENCES               |
    // +--------------------------------------------------+

    /// <summary>Global UI sound state.</summary>
    public bool SoundEnabled { get; set; } = true;

    /// <summary>"System" | "Light" | "Dark"</summary>
    public string Theme { get; set; } = "System";
}
