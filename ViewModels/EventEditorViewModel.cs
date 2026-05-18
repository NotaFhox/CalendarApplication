using Calender.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calender.ViewModels;

/// <summary>
/// Backs the EventEditorDialog form.
/// Call Reset() before showing for a new event, LoadFrom() before showing for an edit.
/// Call ToCalendarEvent() after the dialog closes to retrieve the populated entity.
/// </summary>
public partial class EventEditorViewModel : ObservableObject
{
    // +--------------------------------------------------+
    // |                    FIELDS                        |
    // +--------------------------------------------------+

    private CalendarEvent? _editingEvent;

    // +--------------------------------------------------+
    // |                   PROPERTIES                     |
    // +--------------------------------------------------+

    /// <summary>True when the dialog was opened for an existing event.</summary>
    public bool IsEditMode => _editingEvent is not null;

    // WinUI 3: DatePicker.Date binds to DateTimeOffset; TimePicker.Time binds to TimeSpan.
    [ObservableProperty] private string        _title       = string.Empty;
    [ObservableProperty] private string?       _description;
    [ObservableProperty] private string?       _location;
    [ObservableProperty] private string        _color       = "#0078D4";
    [ObservableProperty] private bool          _isAllDay;
    [ObservableProperty] private DateTimeOffset _startDate  = DateTimeOffset.Now;
    [ObservableProperty] private TimeSpan       _startTime  = DateTime.Now.TimeOfDay;
    [ObservableProperty] private DateTimeOffset _endDate    = DateTimeOffset.Now;
    [ObservableProperty] private TimeSpan       _endTime    = DateTime.Now.AddHours(1).TimeOfDay;

    // +--------------------------------------------------+
    // |                   COMMANDS                       |
    // +--------------------------------------------------+

    [RelayCommand]
    private void SelectColor(string hex) => Color = hex;

    // +--------------------------------------------------+
    // |                DIALOG LIFECYCLE                  |
    // +--------------------------------------------------+

    /// <summary>Resets the form for creating a new event. Optionally pre-fills the date.</summary>
    public void Reset(DateTimeOffset? suggestedDate = null)
    {
        _editingEvent = null;
        var date = suggestedDate ?? DateTimeOffset.Now;

        Title       = string.Empty;
        Description = null;
        Location    = null;
        Color       = "#0078D4";
        IsAllDay    = false;
        StartDate   = new DateTimeOffset(date.Date, TimeSpan.Zero);
        StartTime   = DateTime.Now.TimeOfDay;
        EndDate     = new DateTimeOffset(date.Date, TimeSpan.Zero);
        EndTime     = DateTime.Now.AddHours(1).TimeOfDay;
    }

    /// <summary>Populates the form from an existing event for editing.</summary>
    public void LoadFrom(CalendarEvent evt)
    {
        _editingEvent = evt;
        Title         = evt.Title;
        Description   = evt.Description;
        Location      = evt.Location;
        Color         = evt.Color;
        IsAllDay      = evt.IsAllDay;
        StartDate     = new DateTimeOffset(evt.StartTime.Date, TimeSpan.Zero);
        StartTime     = evt.StartTime.TimeOfDay;
        EndDate       = new DateTimeOffset(evt.EndTime.Date, TimeSpan.Zero);
        EndTime       = evt.EndTime.TimeOfDay;
    }

    /// <summary>Merges form state back into an entity (creates a new one in create mode).</summary>
    public CalendarEvent ToCalendarEvent()
    {
        var evt         = _editingEvent ?? new CalendarEvent();
        evt.Title       = Title;
        evt.Description = Description;
        evt.Location    = Location;
        evt.Color       = Color;
        evt.IsAllDay    = IsAllDay;

        evt.StartTime = IsAllDay
            ? StartDate.Date
            : StartDate.Date + StartTime;

        evt.EndTime = IsAllDay
            ? EndDate.Date.AddDays(1).AddSeconds(-1)
            : EndDate.Date + EndTime;

        return evt;
    }
}
