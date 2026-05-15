using Calender.Models;
using Calender.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Calender.ViewModels;

/// <summary>Date-group header row in the Agenda flat list.</summary>
public sealed class AgendaHeader
{
    public DateTime Date { get; init; }

    /// <summary>Human-readable label — "Today, May 11" for today, "Monday, May 12" otherwise.</summary>
    public string Display => Date.Date == DateTime.Today
        ? $"Today, {Date:MMMM d}"
        : Date.ToString("dddd, MMMM d");

    public bool IsToday => Date.Date == DateTime.Today;

    /// <summary>Full opacity for today's header; slightly muted for future dates.</summary>
    public double HeaderOpacity => IsToday ? 1.0 : 0.65;
}

/// <summary>
/// ViewModel for AgendaPage.
/// Builds a flat mixed list of AgendaHeader + CalendarEvent objects
/// covering the next 60 days (today included).
/// </summary>
public partial class AgendaViewModel : ObservableObject
{
    private readonly CalendarService _service = new();

    [ObservableProperty]
    private ObservableCollection<object> _rows = [];

    [ObservableProperty]
    private bool _isEmpty;

    public AgendaViewModel()
    {
        _ = LoadAsync();
    }

    public async Task LoadAsync()
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
