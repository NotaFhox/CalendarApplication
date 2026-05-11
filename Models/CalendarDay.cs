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
    private List<CalendarEvent> _events = [];

    // Only the first 3 events are shown as chips; the rest become "+N more"
    public List<CalendarEvent> VisibleEvents => Events.Take(3).ToList();
    public int    OverflowCount => Math.Max(0, Events.Count - 3);
    public bool   HasOverflow   => OverflowCount > 0;
    public string OverflowLabel => $"+{OverflowCount} more";
}
