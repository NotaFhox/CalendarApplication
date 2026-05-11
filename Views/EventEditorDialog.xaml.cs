using Calender.Models;
using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class EventEditorDialog : ContentDialog
{
    public EventEditorViewModel ViewModel { get; } = new();

    // Stored so the caller can pass it to DeleteEventCommand if the
    // secondary "Delete" button is pressed.
    public CalendarEvent? OriginalEvent { get; private set; }

    public EventEditorDialog()
    {
        this.InitializeComponent();
    }

    /// Call before ShowAsync() when creating a new event.
    /// suggestedDate pre-fills the date pickers (defaults to today).
    public void PrepareForCreate(DateTimeOffset? suggestedDate = null)
    {
        OriginalEvent       = null;
        SecondaryButtonText = string.Empty; // hide Delete button
        Title               = "New Event";
        ViewModel.Reset(suggestedDate);
    }

    /// Call before ShowAsync() when editing an existing event.
    public void PrepareForEdit(CalendarEvent evt)
    {
        OriginalEvent       = evt;
        SecondaryButtonText = "Delete";
        Title               = "Edit Event";
        ViewModel.LoadFrom(evt);
    }
}
