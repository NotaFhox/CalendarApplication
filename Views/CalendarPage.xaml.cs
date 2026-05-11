using Calender.Models;
using Calender.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;

namespace Calender.Views;

public sealed partial class CalendarPage : Page
{
    public CalendarViewModel ViewModel { get; } = new();

    // Single dialog instance reused across open/close cycles
    private readonly EventEditorDialog _editorDialog = new();

    // Tracks the previously displayed month so we know which direction to animate
    private DateTime _previousMonth;

    public CalendarPage()
    {
        this.InitializeComponent();
        ViewModel.EventEditorRequested += OnEventEditorRequested;

        _previousMonth = ViewModel.DisplayedMonth;
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    // ── Month-transition animation ────────────────────────────────────────────

    private void ViewModel_PropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ViewModel.DisplayedMonth)) return;

        bool forward = ViewModel.DisplayedMonth > _previousMonth;
        _previousMonth = ViewModel.DisplayedMonth;
        AnimateCalendarTransition(forward);
    }

    /// <summary>
    /// Slides the calendar grid in from the leading edge (next month) or
    /// trailing edge (previous month) using a short CubicEase translate.
    /// </summary>
    private void AnimateCalendarTransition(bool forward)
    {
        const double Offset   = 60.0;   // px the grid starts off-center
        const int    Duration = 250;    // ms for the slide

        // Instantly position the grid off to the incoming side…
        CalendarSlide.X = forward ? Offset : -Offset;

        // …then smoothly slide it back to centre
        var anim = new DoubleAnimation
        {
            To                      = 0,
            Duration                = new Duration(TimeSpan.FromMilliseconds(Duration)),
            EasingFunction          = new CubicEase { EasingMode = EasingMode.EaseOut },
            EnableDependentAnimation = true,
        };

        Storyboard.SetTarget(anim, CalendarSlide);
        Storyboard.SetTargetProperty(anim, "X");

        var sb = new Storyboard();
        sb.Children.Add(anim);
        sb.Begin();
    }

    // ── Dialog orchestration ──────────────────────────────────────────────────

    private async void OnEventEditorRequested(object? sender, CalendarEvent? evt)
    {
        // XamlRoot must be set before ShowAsync — it changes on every navigation
        _editorDialog.XamlRoot = this.XamlRoot;

        if (evt is not null)
            _editorDialog.PrepareForEdit(evt);
        else
            _editorDialog.PrepareForCreate();

        var result = await _editorDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var calEvent = _editorDialog.ViewModel.ToCalendarEvent();

            if (_editorDialog.ViewModel.IsEditMode)
                await ViewModel.UpdateEventCommand.ExecuteAsync(calEvent);
            else
                await ViewModel.CreateEventCommand.ExecuteAsync(calEvent);
        }
        else if (result == ContentDialogResult.Secondary
              && _editorDialog.OriginalEvent is { } toDelete)
        {
            await ViewModel.DeleteEventCommand.ExecuteAsync(toDelete);
        }
    }

    // ── Chip interaction handlers ─────────────────────────────────────────────

    // Tap on chip body → open edit dialog
    private void ChipButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarEvent evt)
            ViewModel.RequestEditEvent(evt);
        e.Handled = true; // stop tap from bubbling to the day cell
    }

    // Context-menu "Edit" item
    private void ChipEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarEvent evt)
            ViewModel.RequestEditEvent(evt);
    }

    // Context-menu "Delete" item — no dialog, immediate delete
    private void ChipDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarEvent evt)
            _ = ViewModel.DeleteEventCommand.ExecuteAsync(evt);
    }
}
