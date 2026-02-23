using Calender.Models;
using Microsoft.EntityFrameworkCore;

namespace Calender.Data;

public class AppDbContext : DbContext
{
    public DbSet<CalendarEvent> Events { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        // Store the database in %LOCALAPPDATA%\Calender\calendar.db
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Calender");

        Directory.CreateDirectory(folder);

        options.UseSqlite($"Data Source={Path.Combine(folder, "calendar.db")}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Color).HasDefaultValue("#0078D4");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now')");
        });
    }
}
