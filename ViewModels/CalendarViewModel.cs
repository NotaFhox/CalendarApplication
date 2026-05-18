using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Calender.ViewModels;

public partial class CalendarViewModel : ObservableObject
{
    // +--------------------------------------------------+
    // |                    FIELDS                        |
    // +--------------------------------------------------+

    private readonly CalendarService _service = new();

    // +--------------------------------------------------+
    // |                   PROPERTIES                     |
    // +--------------------------------------------------+

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MonthYearDisplay))]
    private DateTime _displayedMonth = new(DateTime.Today.Year, DateTime.Today.Month, 1);

    public string MonthYearDisplay => DisplayedMonth.ToString("MMMM yyyy");

    [ObservableProperty] private ObservableCollection<CalendarDay> _calendarDays = [];
    [ObservableProperty] private CalendarDay?                      _selectedDay;

    // +--------------------------------------------------+
    // |                    EVENTS                        |
    // +--------------------------------------------------+

    /// <summary>
    /// Raised when a page should open the EventEditorDialog.
    /// null = create new event; non-null = edit existing event.
    /// </summary>
    public event EventHandler<CalendarEvent?>? EventEditorRequested;

    /// <summary>Date to pre-fill when the dialog opens for a new event (null = today).</summary>
    public DateTime? PendingCreateDate { get; private set; }

    // +--------------------------------------------------+
    // |                  CONSTRUCTION                    |
    // +--------------------------------------------------+

    public CalendarViewModel()
    {
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

    // +--------------------------------------------------+
    // |              NAVIGATION COMMANDS                 |
    // +--------------------------------------------------+

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
    private void GoToToday()
    {
        DisplayedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        BuildCalendarGrid();
        _ = LoadEventsAsync();
    }

    // +--------------------------------------------------+
    // |               EDITOR COMMANDS                    |
    // +--------------------------------------------------+

    [RelayCommand]
    private void RequestNewEvent()
    {
        PendingCreateDate = null;
        EventEditorRequested?.Invoke(this, null);
    }

    /// <summary>Opens the create dialog with a specific date pre-filled.</summary>
    public void RequestNewEventOnDate(DateTime date)
    {
        PendingCreateDate = date;
        EventEditorRequested?.Invoke(this, null);
    }

    public void RequestEditEvent(CalendarEvent evt)
        => EventEditorRequested?.Invoke(this, evt);

    // +--------------------------------------------------+
    // |                 CRUD COMMANDS                    |
    // +--------------------------------------------------+

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

    // +--------------------------------------------------+
    // |                 DATA LOADING                     |
    // +--------------------------------------------------+

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        if (CalendarDays.Count == 0) return;

        // Use the full 6-week grid range so multi-day events that begin before
        // the displayed month still appear on every day they span.
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

    // +--------------------------------------------------+
    // |                  GRID BUILDER                    |
    // +--------------------------------------------------+

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
