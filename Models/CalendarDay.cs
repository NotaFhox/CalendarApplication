namespace Calender.Models;

public class CalendarDay
{
    public DateTime          Date           { get; set; }

    public bool              IsCurrentMonth { get; set; }
    public bool              IsToday        => Date.Date == DateTime.Today;
    public List<CalendarEvent> Events       { get; set; } = [];
}
