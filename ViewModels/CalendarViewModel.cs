using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Calender.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    private readonly CalendarService _service = new();

    // ── Event raised when the Page should open the EventEditorDialog.
    //    null  → create new event
    //    value → edit existing event
    public event EventHandler<CalendarEvent?>? EventEditorRequested;

    // ── State ─────────────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthYearDisplay))]
    private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public string MonthYearDisplay => DisplayedMonth.ToString("MMMM yyyy");

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = [];

    [ObservableProperty]
    private CalendarDay? _selectedDay;

    // ── Constructor ───────────────────────────────────────────────────────────

    public CalendarViewModel()
    {
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

    // ── Navigation commands ───────────────────────────────────────────────────

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

    // ── Data loading ──────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        var start  = DisplayedMonth;
        var end    = DisplayedMonth.AddMonths(1);
        var events = await _service.GetEventsAsync(start, end);

        foreach (var day in CalendarDays)
            day.Events = events
                .Where(e => e.StartTime.Date == day.Date.Date)
                .ToList();
    }

    // ── Editor trigger (called by the Page's event handlers) ─────────────────

    [RelayCommand]
    private void RequestNewEvent() => EventEditorRequested?.Invoke(this, null);

    public void RequestEditEvent(CalendarEvent evt) =>
        EventEditorRequested?.Invoke(this, evt);

    // ── CRUD commands ─────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task CreateEventAsync(CalendarEvent evt)
    {
        await _service.CreateAsync(evt);
        await LoadEventsAsync();
    }

    [RelayCommand]
    private async Task UpdateEventAsync(CalendarEvent evt)
    {
        await _service.UpdateAsync(evt);
        await LoadEventsAsync();
    }

    [RelayCommand]
    private async Task DeleteEventAsync(CalendarEvent evt)
    {
        await _service.DeleteAsync(evt.Id);
        await LoadEventsAsync();
    }

    // ── Grid builder ─────────────────────────────────────────────────────────

    private void BuildCalendarGrid()
    {
        CalendarDays.Clear();
        var firstOfMonth = DisplayedMonth;
        var gridStart    = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);

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
