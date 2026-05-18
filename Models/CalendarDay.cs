using CommunityToolkit.Mvvm.ComponentModel;

namespace Calender.Models;

public partial class CalendarDay : ObservableObject
{
    public DateTime Date           { get; set; }
    public bool     IsCurrentMonth { get; set; }
    public bool     IsToday        => Date.Date == DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisibleEvents))]
    [NotifyPropertyChangedFor(nameof(OverflowCount))]
    [NotifyPropertyChangedFor(nameof(HasOverflow))]
    [NotifyPropertyChangedFor(nameof(OverflowLabel))]
    [NotifyPropertyChangedFor(nameof(HasAnyEvents))]
    private List<CalendarEvent> _events = [];

    // Only the first 3 events are shown as chips; the rest become "+N more"
    public List<CalendarEvent> VisibleEvents  => Events.Take(3).ToList();
    public bool                HasAnyEvents   => Events.Count > 0;
    public int    OverflowCount => Math.Max(0, Events.Count - 3);
    public bool   HasOverflow   => OverflowCount > 0;
    public string OverflowLabel => $"+{OverflowCount} more";

    /// <summary>Human-readable date for the day-detail flyout header.</summary>
    public string DateLabel => Date.ToString("dddd, MMMM d");

    /// <summary>True for Saturday and Sunday — used to tint weekend columns.</summary>
    public bool IsWeekend =>
        Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
