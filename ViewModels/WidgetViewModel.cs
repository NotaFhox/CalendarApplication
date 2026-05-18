using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;

namespace Calender.ViewModels;

/// <summary>
/// Shared ViewModel for both SmallWidgetView and LargeWidgetView.
/// Owned by WidgetWindow; refreshes every 5 minutes via DispatcherTimer.
/// Call Dispose() when the widget window closes to stop the timer.
/// </summary>
public partial class WidgetViewModel : ObservableObject, IDisposable
{
    private readonly CalendarService _service = new();
    private readonly DispatcherTimer _timer;

    // ── Today display ─────────────────────────────────────────────────────────

    [ObservableProperty] private DateTime _today = DateTime.Today;

    public string DayName   => Today.ToString("dddd");
    public string MonthYear => Today.ToString("MMMM yyyy");

    // ── Events ────────────────────────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<CalendarEvent> _todayEvents    = [];
    [ObservableProperty] private ObservableCollection<CalendarEvent> _upcomingEvents = [];

    public bool HasNoTodayEvents    => TodayEvents.Count    == 0;
    public bool HasNoUpcomingEvents => UpcomingEvents.Count == 0;

    // ── Mini-calendar (large widget) ──────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthYearDisplay))]
    private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    [ObservableProperty] private ObservableCollection<CalendarDay> _calendarDays = [];

    public string MonthYearDisplay => DisplayedMonth.ToString("MMMM yyyy");

    // ── Constructor ───────────────────────────────────────────────────────────

    public WidgetViewModel()
    {
        BuildCalendarGrid();
        _ = RefreshAsync();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
        _timer.Tick += async (_, _) => await RefreshAsync();
        _timer.Start();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task RefreshAsync()
    {
        Today = DateTime.Today;
        OnPropertyChanged(nameof(DayName));
        OnPropertyChanged(nameof(MonthYear));

        // Use the full 6-week grid range so multi-day events that started before
        // the displayed month are still fetched and placed on all their days.
        var gridStart = CalendarDays.Count > 0 ? CalendarDays.First().Date : DisplayedMonth;
        var gridEnd   = CalendarDays.Count > 0 ? CalendarDays.Last().Date.AddDays(1)
                                               : DisplayedMonth.AddMonths(1);
        var events = await _service.GetEventsAsync(gridStart, gridEnd);

        // Today's events — include multi-day events that span today
        TodayEvents.Clear();
        foreach (var e in events
            .Where(e => e.StartTime.Date <= Today.Date && e.EndTime.Date >= Today.Date)
            .OrderBy(e => e.StartTime))
            TodayEvents.Add(e);

        // Next 5 upcoming (starting strictly after today)
        UpcomingEvents.Clear();
        foreach (var e in events.Where(e => e.StartTime.Date > Today.Date).Take(5))
            UpcomingEvents.Add(e);

        // Distribute events into calendar-day dots (multi-day events appear on every day they span)
        foreach (var day in CalendarDays)
            day.Events = events
                .Where(e => e.StartTime.Date <= day.Date.Date && e.EndTime.Date >= day.Date.Date)
                .ToList();

        OnPropertyChanged(nameof(HasNoTodayEvents));
        OnPropertyChanged(nameof(HasNoUpcomingEvents));
    }

    // ── Grid builder ──────────────────────────────────────────────────────────

    private void BuildCalendarGrid()
    {
        CalendarDays.Clear();
        var gridStart = DisplayedMonth.AddDays(-(int)DisplayedMonth.DayOfWeek);

        for (var i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i);
            CalendarDays.Add(new CalendarDay
            {
                Date           = date,
                IsCurrentMonth = date.Month == DisplayedMonth.Month,
            });
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────────

    public void Dispose() => _timer.Stop();
}
