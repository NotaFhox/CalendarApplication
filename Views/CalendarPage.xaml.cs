using Calender.Models;
using Calender.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;
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

    /// <summary>Smooth compositor scale on the nav-bar buttons.</summary>
    private static void ScaleButton(UIElement element, float target)
    {
        var visual     = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        // Pin the scale origin to the element's centre once it has a size
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
                // "Show" = a gentle chime; much softer than SystemAsterisk
                ElementSoundPlayer.Play(ElementSoundKind.Show);
            }
        }
        else if (result == ContentDialogResult.Secondary
              && _editorDialog.OriginalEvent is { } toDelete)
        {
            await ViewModel.DeleteEventCommand.ExecuteAsync(toDelete);
            // "Hide" = a quiet dismiss cue; not an alarm sound
            ElementSoundPlayer.Play(ElementSoundKind.Hide);
        }
    }

    // ── Overflow flyout ("+N more" button) ───────────────────────────────────

    private void OverflowBtn_Tapped(object sender, TappedRoutedEventArgs e)
    {
        e.Handled = true; // prevent DayCell_Tapped from also firing
        if (sender is not FrameworkElement { Tag: CalendarDay day }) return;
        ShowDayFlyout((FrameworkElement)sender, day);
    }

    /// <summary>
    /// Builds a Flyout at runtime listing every event on the given day.
    /// Each entry can be tapped to edit or right-clicked for a context menu.
    /// </summary>
    private void ShowDayFlyout(FrameworkElement anchor, CalendarDay day)
    {
        var panel = new StackPanel { Spacing = 2, MinWidth = 220, MaxWidth = 300 };

        // Date header
        panel.Children.Add(new TextBlock
        {
            Text       = day.DateLabel,
            FontSize   = 13,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Margin     = new Thickness(4, 0, 4, 8),
        });

        Flyout? flyout = null; // forward-reference so row buttons can close it

        foreach (var evt in day.Events)
        {
            var row = BuildEventRow(evt, () => flyout?.Hide());
            panel.Children.Add(row);
        }

        flyout = new Flyout
        {
            Content   = panel,
            Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom,
        };
        flyout.ShowAt(anchor);
    }

    private UIElement BuildEventRow(CalendarEvent evt, Action closeFlyout)
    {
        // Colour dot
        var dot = new Ellipse
        {
            Width               = 8,
            Height              = 8,
            VerticalAlignment   = VerticalAlignment.Center,
            Fill                = HexToBrush(evt.Color),
        };

        // Title + time stack
        var textPanel = new StackPanel();
        textPanel.Children.Add(new TextBlock
        {
            Text           = evt.Title,
            FontSize       = 12,
            TextTrimming   = Microsoft.UI.Xaml.TextTrimming.CharacterEllipsis,
            MaxLines       = 1,
        });
        textPanel.Children.Add(new TextBlock
        {
            Text    = evt.StartTimeShort,
            FontSize = 10,
            Opacity = 0.55,
        });

        var hPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        hPanel.Children.Add(dot);
        hPanel.Children.Add(textPanel);

        var btn = new Button
        {
            Content             = hPanel,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background          = new SolidColorBrush(Colors.Transparent),
            BorderThickness     = new Thickness(0),
            Padding             = new Thickness(6, 5, 6, 5),
            Margin              = new Thickness(0, 1, 0, 1),
        };
        ToolTipService.SetToolTip(btn, evt.ChipTooltip);

        btn.Tapped += (_, te) =>
        {
            te.Handled = true;
            closeFlyout();
            ViewModel.RequestEditEvent(evt);
        };

        // Context menu
        var editItem = new MenuFlyoutItem
        {
            Text = "Edit",
            Icon = new FontIcon { Glyph = "" },
        };
        editItem.Click += (_, _) => { closeFlyout(); ViewModel.RequestEditEvent(evt); };

        var deleteItem = new MenuFlyoutItem
        {
            Text = "Delete",
            Icon = new FontIcon { Glyph = "", Foreground = new SolidColorBrush(Colors.Firebrick) },
        };
        deleteItem.Click += (_, _) =>
        {
            closeFlyout();
            _ = ViewModel.DeleteEventCommand.ExecuteAsync(evt);
        };

        var menu = new MenuFlyout();
        menu.Items.Add(editItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(deleteItem);
        btn.ContextFlyout = menu;

        return btn;
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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SolidColorBrush HexToBrush(string hex)
    {
        try
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex[..2], 16);
            byte g = Convert.ToByte(hex[2..4], 16);
            byte b = Convert.ToByte(hex[4..6], 16);
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
        }
        catch
        {
            return new SolidColorBrush(Colors.SteelBlue);
        }
    }
}
