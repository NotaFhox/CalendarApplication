using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Calender.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly CalendarService _service = new();

    // ── Displayed month ───────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthYearDisplay))]
    private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public string MonthYearDisplay => DisplayedMonth.ToString("MMMM yyyy");

    // ── Calendar grid cells ───────────────────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = [];

    // ── Selected event (for detail panel) ────────────────────────────────────

    [ObservableProperty]
    private CalendarEvent? _selectedEvent;

    // ── Constructor ───────────────────────────────────────────────────────────

    public CalendarViewModel()
    {
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void PreviousMonth()
    {
        DisplayedMonth = DisplayedMonth.AddMonths(-1);
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private void NextMonth()
    {
        DisplayedMonth = DisplayedMonth.AddMonths(1);
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        var start = DisplayedMonth;
        var end   = DisplayedMonth.AddMonths(1);
        var events = await _service.GetEventsAsync(start, end);

        
        foreach (var day in CalendarDays)
        {
            day.Events = events
                .Where(e => e.StartTime.Date == day.Date.Date)
                .ToList();
        }
    }

    
    private void BuildCalendarGrid()
    {
        CalendarDays.Clear();

        var firstOfMonth = DisplayedMonth;
        // Rewind to the Sunday that starts the first visible week
        var gridStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);

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
}
