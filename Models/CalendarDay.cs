using CommunityToolkit.Mvvm.ComponentModel;

namespace Calender.Models;

/// <summary>UI grid-cell model — one instance per cell in the 6-week calendar grid.</summary>
public partial class CalendarDay : ObservableObject
{
    // +--------------------------------------------------+
    // |                   PROPERTIES                     |
    // +--------------------------------------------------+

    public DateTime Date           { get; set; }
    public bool     IsCurrentMonth { get; set; }
    public bool     IsToday        => Date.Date == DateTime.Today;

    /// <summary>True for Saturday and Sunday — used to tint weekend columns.</summary>
    public bool IsWeekend =>
        Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    /// <summary>Human-readable date for the day-detail flyout header.</summary>
    public string DateLabel => Date.ToString("dddd, MMMM d");

    // +--------------------------------------------------+
    // |                 OBSERVABLE EVENTS                |
    // +--------------------------------------------------+

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisibleEvents))]
    [NotifyPropertyChangedFor(nameof(OverflowCount))]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    [NotifyPropertyChangedFor(nameof(OverflowLabel))]
    [NotifyPropertyChangedFor(nameof(HasAnyEvents))]
    private List<CalendarEvent> _events = [];

    // +--------------------------------------------------+
    // |               COMPUTED FROM EVENTS               |
    // +--------------------------------------------------+

    /// <summary>First 3 events shown as inline chips.</summary>
    public List<CalendarEvent> VisibleEvents => Events.Take(3).ToList();

    public bool   HasAnyEvents  => Events.Count > 0;
    public int    OverflowCount => Math.Max(0, Events.Count - 3);
    public bool   HasOverflow   => OverflowCount > 0;
    public string OverflowLabel => $"+{OverflowCount} more";
}
