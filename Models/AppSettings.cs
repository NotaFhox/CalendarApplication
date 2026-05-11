namespace Calender.Models;

/// <summary>
/// Persisted user preferences — stored alongside the SQLite DB in
/// %LOCALAPPDATA%\Calender\settings.json.
/// </summary>
public class AppSettings
{
    /// <summary>Widget left edge position in screen pixels.</summary>
    public int    WidgetX    { get; set; } = 80;
    /// <summary>Widget top edge position in screen pixels.</summary>
    public int    WidgetY    { get; set; } = 80;
    /// <summary>"Small" | "Large"</summary>
    public string WidgetSize { get; set; } = "Large";
}
