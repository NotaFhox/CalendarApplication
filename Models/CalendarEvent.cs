namespace Calender.Models;

/// <summary>
/// EF Core entity — one row per calendar event stored in SQLite.
/// </summary>
public class CalendarEvent
{
    public int      Id          { get; set; }
    public string   Title       { get; set; } = string.Empty;
    public string?  Description { get; set; }
    public DateTime StartTime   { get; set; }
    public DateTime EndTime     { get; set; }
    public bool     IsAllDay    { get; set; }
    public string?  Location    { get; set; }
    /// <summary>Hex color used to tint the event chip, e.g. "#0078D4".</summary>
    public string   Color       { get; set; } = "#0078D4";
    public DateTime CreatedAt   { get; set; } = DateTime.Now;
    public DateTime UpdatedAt   { get; set; } = DateTime.Now;

    /// <summary>Short time label shown in the large widget's event list.</summary>
    public string StartTimeShort => IsAllDay ? "All day" : StartTime.ToString("h:mm tt");

    /// <summary>Single-line tooltip shown on calendar chips: title + time (+ location if set).</summary>
    public string ChipTooltip
    {
        get
        {
            var s = IsAllDay ? $"{Title}  All day" : $"{Title}  {StartTimeShort}";
            return !string.IsNullOrWhiteSpace(Location) ? $"{s}  |  {Location}" : s;
        }
    }
}
