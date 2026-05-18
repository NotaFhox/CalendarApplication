using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Calender.ViewModels;

// +--------------------------------------------------+
// |                  AGENDA HEADER                   |
// +--------------------------------------------------+

/// <summary>Date-group header row in the Agenda flat list.</summary>
public sealed class AgendaHeader
{
    public DateTime Date { get; init; }

    /// <summary>"Today, May 11" for today; "Monday, May 12" for any other date.</summary>
    public string Display => Date.Date == DateTime.Today
        ? $"Today, {Date:MMMM d}"
        : Date.ToString("dddd, MMMM d");

    public bool   IsToday       => Date.Date == DateTime.Today;
    public double HeaderOpacity => IsToday ? 1.0 : 0.65;
}

// +--------------------------------------------------+
// |                 AGENDA VIEW MODEL                |
// +--------------------------------------------------+

/// <summary>
/// Builds a flat mixed list of AgendaHeader + CalendarEvent objects
/// covering the next 60 days starting from today.
/// </summary>
public partial class AgendaViewModel : ObservableObject
{
    // +--------------------------------------------------+
    // |                    FIELDS                        |
    // +--------------------------------------------------+

    private readonly CalendarService _service = new();

    // +--------------------------------------------------+
    // |                   PROPERTIES                     |
    // +--------------------------------------------------+

    [ObservableProperty] private ObservableCollection<object> _rows    = [];
    [ObservableProperty] private bool                         _isEmpty;

    // +--------------------------------------------------+
    // |                  CONSTRUCTION                    |
    // +--------------------------------------------------+

    public AgendaViewModel() => _ = LoadAgendaAsync();

    // +--------------------------------------------------+
    // |                  DATA LOADING                    |
    // +--------------------------------------------------+

    public async Task LoadAgendaAsync()
    {
        var start  = DateTime.Today;
        var end    = start.AddDays(60);
        var events = await _service.GetEventsAsync(start, end);

        var grouped = events
            .GroupBy(e => e.StartTime.Date)
            .OrderBy(g => g.Key);

        Rows.Clear();
        foreach (var group in grouped)
        {
            Rows.Add(new AgendaHeader { Date = group.Key });
            foreach (var evt in group.OrderBy(e => e.StartTime))
                Rows.Add(evt);
        }

        IsEmpty = Rows.Count == 0;
    }
}
