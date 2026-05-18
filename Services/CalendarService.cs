using Calender.Data;
using Calender.Models;
using Microsoft.EntityFrameworkCore;

namespace Calender.Services;

/// <summary>Async CRUD operations over CalendarEvent via AppDbContext.</summary>
public class CalendarService
{
    // +--------------------------------------------------+
    // |                     READ                         |
    // +--------------------------------------------------+

    /// <summary>
    /// Returns all events whose time range overlaps [start, end).
    /// This correctly includes multi-day events that begin before <paramref name="start"/>.
    /// </summary>
    public async Task<List<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end)
    {
        await using var ctx = new AppDbContext();
        return await ctx.Events
            .Where(e => e.StartTime < end && e.EndTime >= start)
            .OrderBy(e => e.StartTime)
            .ToListAsync();
    }

    // +--------------------------------------------------+
    // |                    WRITE                         |
    // +--------------------------------------------------+

    public async Task<CalendarEvent> CreateAsync(CalendarEvent evt)
    {
        await using var ctx = new AppDbContext();
        evt.CreatedAt = evt.UpdatedAt = DateTime.Now;
        ctx.Events.Add(evt);
        await ctx.SaveChangesAsync();
        return evt;
    }

    public async Task UpdateAsync(CalendarEvent evt)
    {
        await using var ctx = new AppDbContext();
        evt.UpdatedAt = DateTime.Now;
        ctx.Events.Update(evt);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var ctx = new AppDbContext();
        var evt = await ctx.Events.FindAsync(id);
        if (evt is not null)
        {
            ctx.Events.Remove(evt);
            await ctx.SaveChangesAsync();
        }
    }
}
