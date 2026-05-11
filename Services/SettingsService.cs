using Calender.Models;
using System.Text.Json;

namespace Calender.Services;

/// <summary>
/// Loads and saves AppSettings as JSON beside the SQLite database.
/// All failures are swallowed — the app always gets a valid settings object.
/// </summary>
public class SettingsService
{
    private static string SettingsPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Calender", "settings.json");

    private static readonly JsonSerializerOptions _writeOpts = new() { WriteIndented = true };

    public AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return JsonSerializer.Deserialize<AppSettings>(
                    File.ReadAllText(SettingsPath)) ?? new AppSettings();
        }
        catch { /* fall through */ }
        return new AppSettings();
    }

    public void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, JsonSerializer.Serialize(settings, _writeOpts));
        }
        catch { /* best-effort */ }
    }
}
