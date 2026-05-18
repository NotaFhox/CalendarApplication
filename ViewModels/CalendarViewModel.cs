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
        if (CalendarDays.Count == 0) return;

        // Use the full grid range so multi-day events starting before the month
        // (e.g. Dec 29 in a January grid) are still fetched and placed correctly.
        var gridStart = CalendarDays.First().Date;
        var gridEnd   = CalendarDays.Last().Date.AddDays(1);
        var events    = await _service.GetEventsAsync(gridStart, gridEnd);

        foreach (var day in CalendarDays)
            day.Events = events
                .Where(e => e.StartTime.Date <= day.Date.Date
                         && e.EndTime.Date   >= day.Date.Date)
                .OrderBy(e => e.StartTime)
                .ToList();
    }

    // ── Editor trigger (called by the Page's event handlers) ─────────────────

    /// <summary>Date to pre-fill when opening the dialog for a new event (null = today).</summary>
    public DateTime? PendingCreateDate { get; private set; }

    [RelayCommand]
    private void RequestNewEvent()
    {
        PendingCreateDate = null;
        EventEditorRequested?.Invoke(this, null);
    }

    /// <summary>Opens the create dialog with the given date pre-filled.</summary>
    public void RequestNewEventOnDate(DateTime date)
    {
        PendingCreateDate = date;
        EventEditorRequested?.Invoke(this, null);
    }

    public void RequestEditEvent(CalendarEvent evt) =>
        EventEditorRequested?.Invoke(this, evt);

    [RelayCommand]
    private void GoToToday()
    {
        DisplayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

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
