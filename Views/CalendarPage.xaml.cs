using Calender.Models;
using Calender.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System.Numerics;

namespace Calender.Views;

public sealed partial class CalendarPage : Page
{
    public CalendarViewModel ViewModel { get; } = new();

    private readonly EventEditorDialog _editorDialog = new();
    private DateTime _previousMonth;

    // ── Constructor ───────────────────────────────────────────────────────────

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

    private void AnimateCalendarTransition(bool forward)
    {
        const double Offset   = 60.0;
        const int    Duration = 250;

        CalendarSlide.X      = forward ? Offset : -Offset;
        CalendarGrid.Opacity = 0;

        var slideAnim = new DoubleAnimation
        {
            To                       = 0,
            Duration                 = new Duration(TimeSpan.FromMilliseconds(Duration)),
            EasingFunction           = new CubicEase { EasingMode = EasingMode.EaseOut },
            EnableDependentAnimation = true,
        };
        Storyboard.SetTarget(slideAnim, CalendarSlide);
        Storyboard.SetTargetProperty(slideAnim, "X");

        var fadeAnim = new DoubleAnimation
        {
            From           = 0,
            To             = 1,
            Duration       = new Duration(TimeSpan.FromMilliseconds(Duration)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(fadeAnim, CalendarGrid);
        Storyboard.SetTargetProperty(fadeAnim, "Opacity");

        var sb = new Storyboard();
        sb.Children.Add(slideAnim);
        sb.Children.Add(fadeAnim);
        sb.Begin();
    }

    // ── Hover animation + sound ───────────────────────────────────────────────

    private static void ScaleButton(UIElement element, float target)
    {
        var visual     = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        if (element is FrameworkElement fe && fe.ActualWidth > 0)
            visual.CenterPoint = new Vector3(
                (float)fe.ActualWidth  / 2f,
                (float)fe.ActualHeight / 2f,
                0f);

        var ease = compositor.CreateCubicBezierEasingFunction(
            new Vector2(0.25f, 0.46f), new Vector2(0.45f, 1.0f));

        var xAnim = compositor.CreateScalarKeyFrameAnimation();
        xAnim.Duration = TimeSpan.FromMilliseconds(target > 1f ? 130 : 190);
        xAnim.InsertKeyFrame(1f, target, ease);

        var yAnim = compositor.CreateScalarKeyFrameAnimation();
        yAnim.Duration = xAnim.Duration;
        yAnim.InsertKeyFrame(1f, target, ease);

        visual.StartAnimation("Scale.X", xAnim);
        visual.StartAnimation("Scale.Y", yAnim);
    }

    private void NavBtn_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ElementSoundPlayer.Play(ElementSoundKind.Focus);
        if (sender is UIElement el) ScaleButton(el, 1.07f);
    }

    private void NavBtn_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement el) ScaleButton(el, 1.0f);
    }

    // ── Dialog orchestration ──────────────────────────────────────────────────

    private async void OnEventEditorRequested(object? sender, CalendarEvent? evt)
    {
        _editorDialog.XamlRoot = this.XamlRoot;

        if (evt is not null)
        {
            _editorDialog.PrepareForEdit(evt);
        }
        else
        {
            DateTimeOffset? suggested = ViewModel.PendingCreateDate.HasValue
                ? new DateTimeOffset(ViewModel.PendingCreateDate.Value, TimeSpan.Zero)
                : null;
            _editorDialog.PrepareForCreate(suggested);
        }

        var result = await _editorDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var calEvent = _editorDialog.ViewModel.ToCalendarEvent();
            if (_editorDialog.ViewModel.IsEditMode)
            {
                await ViewModel.UpdateEventCommand.ExecuteAsync(calEvent);
            }
            else
            {
                await ViewModel.CreateEventCommand.ExecuteAsync(calEvent);
                ElementSoundPlayer.Play(ElementSoundKind.Show);
            }
        }
        else if (result == ContentDialogResult.Secondary
              && _editorDialog.OriginalEvent is { } toDelete)
        {
            await ViewModel.DeleteEventCommand.ExecuteAsync(toDelete);
            ElementSoundPlayer.Play(ElementSoundKind.Hide);
        }
    }

    // ── Day-cell tap → create event on that date ──────────────────────────────

    private void DayCell_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is DateTime date)
            ViewModel.RequestNewEventOnDate(date);
    }

    // ── Chip interaction handlers ─────────────────────────────────────────────

    private void ChipButton_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarEvent evt)
            ViewModel.RequestEditEvent(evt);
        e.Handled = true;
    }

    private void ChipEdit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarEvent evt)
            ViewModel.RequestEditEvent(evt);
    }

    private void ChipDelete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarEvent evt)
            _ = ViewModel.DeleteEventCommand.ExecuteAsync(evt);
    }
}
