using Calender.Models;
using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class EventEditorDialog : ContentDialog
{
    // +--------------------------------------------------+
    // |                   PROPERTIES                     |
    // +--------------------------------------------------+

    public EventEditorViewModel ViewModel { get; } = new();

    /// <summary>
    /// Stored so the caller can pass it to DeleteEventCommand
    /// when the secondary "Delete" button is pressed.
    /// </summary>
    public CalendarEvent? OriginalEvent { get; private set; }

    // +--------------------------------------------------+
    // |                  CONSTRUCTION                    |
    // +--------------------------------------------------+

    public EventEditorDialog()
    {
        this.InitializeComponent();
    }

    // +--------------------------------------------------+
    // |                DIALOG PREPARATION                |
    // +--------------------------------------------------+

    /// <summary>
    /// Call before ShowAsync() when creating a new event.
    /// <paramref name="suggestedDate"/> pre-fills the date pickers (defaults to today).
    /// </summary>
    public void PrepareForCreate(DateTimeOffset? suggestedDate = null)
    {
        OriginalEvent       = null;
        SecondaryButtonText = string.Empty;
        Title               = "New Event";
        ViewModel.Reset(suggestedDate);
    }

    /// <summary>Call before ShowAsync() when editing an existing event.</summary>
    public void PrepareForEdit(CalendarEvent evt)
    {
        OriginalEvent       = evt;
        SecondaryButtonText = "Delete";
        Title               = "Edit Event";
        ViewModel.LoadFrom(evt);
    }
}
