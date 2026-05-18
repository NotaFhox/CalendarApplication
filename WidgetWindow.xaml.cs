using System.Numerics;
using System.Runtime.InteropServices;
using Calender.Services;
using Calender.ViewModels;
using Calender.Views;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.Graphics;
using WinRT;

namespace Calender;

public sealed partial class WidgetWindow : Window
{
    // +--------------------------------------------------+
    // |                  WIN32 CONSTANTS                 |
    // +--------------------------------------------------+

    private const uint SWP_NOSIZE     = 0x0001;
    private const uint SWP_NOMOVE     = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;

    private static readonly nint HWND_TOPMOST   = new(-1);
    private static readonly nint HWND_NOTOPMOST = new(-2);

    // +--------------------------------------------------+
    // |                    P/INVOKE                      |
    // +--------------------------------------------------+

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetCursorPos(out POINT pt);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(
        nint hWnd, nint hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    // +--------------------------------------------------+
    // |                     FIELDS                       |
    // +--------------------------------------------------+

    private readonly WidgetViewModel           _widgetVm  = new();
    private readonly SettingsService           _settings  = new();
    private readonly AppWindow                 _appWindow;
    private readonly nint                      _hwnd;
    private          DesktopAcrylicController? _acrylicController;

    private bool _isLarge        = true;
    private bool _isPinned       = true;
    private bool _isSoundEnabled = true;

    // Drag state — updated every PointerMoved while dragging
    private bool _isDragging;
    private int  _dragCursorX, _dragCursorY;
    private int  _dragWindowX, _dragWindowY;

    // Widget dimensions in logical pixels (Windows App SDK handles DPI scaling)
    private const int LargeWidth  = 340;
    private const int LargeHeight = 420;
    private const int SmallWidth  = 220;
    private const int SmallHeight = 300;

    // +--------------------------------------------------+
    // |                  CONSTRUCTION                    |
    // +--------------------------------------------------+

    public WidgetWindow()
    {
        this.InitializeComponent();

        _hwnd      = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd));

        SetupBackdrop();
        ConfigureWindow();
        RestorePositionAndSize();
        ShowCurrentView();

        _appWindow.Changed += AppWindow_Changed;
        this.Closed        += (_, _) => OnWidgetClosed();
    }

    // +--------------------------------------------------+
    // |                    BACKDROP                      |
    // +--------------------------------------------------+

    private void SetupBackdrop()
    {
        if (!DesktopAcrylicController.IsSupported()) return;

        var config = new SystemBackdropConfiguration { IsInputActive = true };

        _acrylicController = new DesktopAcrylicController();
        _acrylicController.AddSystemBackdropTarget(
            this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        _acrylicController.SetSystemBackdropConfiguration(config);
    }

    // +--------------------------------------------------+
    // |               WINDOW CONFIGURATION               |
    // +--------------------------------------------------+

    private void ConfigureWindow()
    {
        if (_appWindow.Presenter is OverlappedPresenter p)
            p.SetBorderAndTitleBar(false, false);

        _appWindow.IsShownInSwitchers = false;
    }

    private void RestorePositionAndSize()
    {
        var cfg = _settings.Load();

        _isLarge        = cfg.WidgetSize != "Small";
        _isPinned       = cfg.WidgetAlwaysOnTop;
        _isSoundEnabled = cfg.SoundEnabled;

        int x = cfg.WidgetX > 0 ? cfg.WidgetX : 80;
        int y = cfg.WidgetY > 0 ? cfg.WidgetY : 80;
        int w = _isLarge ? LargeWidth  : SmallWidth;
        int h = _isLarge ? LargeHeight : SmallHeight;

        _appWindow.MoveAndResize(new RectInt32(x, y, w, h));
        ApplyTopmost();
        ApplySoundState();
    }

    private void ApplyTopmost()
    {
        var insertAfter = _isPinned ? HWND_TOPMOST : HWND_NOTOPMOST;
        SetWindowPos(_hwnd, insertAfter, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
    }

    private void ApplySoundState()
    {
        ElementSoundPlayer.State = _isSoundEnabled
            ? ElementSoundPlayerState.On
            : ElementSoundPlayerState.Off;
        UpdateMuteButton();
    }

    // +--------------------------------------------------+
    // |                  VIEW SWITCHING                  |
    // +--------------------------------------------------+

    private void ShowCurrentView()
    {
        WidgetFrame.Content = _isLarge
            ? (object)new LargeWidgetView(_widgetVm)
            : new SmallWidgetView(_widgetVm);

        SizeIcon.Glyph = _isLarge ? "" : "";
        ToolTipService.SetToolTip(SizeBtn, _isLarge ? "Compact view" : "Full view");

        UpdatePinButton();
        UpdateMuteButton();
    }

    private void UpdatePinButton()
    {
        PinIcon.Glyph   = _isPinned ? "" : "";
        PinIcon.Opacity = _isPinned ? 1.0 : 0.45;
        ToolTipService.SetToolTip(PinBtn,
            _isPinned ? "Unpin (allow behind other windows)" : "Pin (always on top)");
    }

    private void UpdateMuteButton()
    {
        MuteIcon.Glyph   = _isSoundEnabled ? "" : "";
        MuteIcon.Opacity = _isSoundEnabled ? 1.0 : 0.45;
        ToolTipService.SetToolTip(MuteBtn,
            _isSoundEnabled ? "Mute sounds" : "Unmute sounds");
    }

    // +--------------------------------------------------+
    // |             POSITION PERSISTENCE                 |
    // +--------------------------------------------------+

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidPositionChange) return;

        var pos = _appWindow.Position;
        var cfg = _settings.Load();
        cfg.WidgetX = pos.X;
        cfg.WidgetY = pos.Y;
        _settings.Save(cfg);
    }

    // +--------------------------------------------------+
    // |              LOAD / FADE-IN HANDLER              |
    // +--------------------------------------------------+

    private void RootBorder_Loaded(object sender, RoutedEventArgs e)
    {
        var fade = new DoubleAnimation
        {
            From           = 0,
            To             = 1,
            Duration       = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(fade, RootBorder);
        Storyboard.SetTargetProperty(fade, "Opacity");

        var sb = new Storyboard();
        sb.Children.Add(fade);
        sb.Begin();
    }

    // +--------------------------------------------------+
    // |                     DRAG                         |
    // +--------------------------------------------------+

    // Manual drag using GetCursorPos physical coordinates. PostMessageW(WM_NCLBUTTONDOWN)
    // is unreliable in WinUI 3 because the framework holds pointer capture at the time
    // PointerPressed fires, preventing the OS window-drag loop from taking over.

    private void DragHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (!e.GetCurrentPoint(null).Properties.IsLeftButtonPressed) return;

        GetCursorPos(out var pt);
        _dragCursorX = pt.X;
        _dragCursorY = pt.Y;

        var pos = _appWindow.Position;
        _dragWindowX = pos.X;
        _dragWindowY = pos.Y;

        (sender as UIElement)?.CapturePointer(e.Pointer);
        _isDragging = true;
        e.Handled   = true;
    }

    private void DragHandle_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (!_isDragging) return;
        GetCursorPos(out var pt);
        _appWindow.Move(new Windows.Graphics.PointInt32(
            _dragWindowX + pt.X - _dragCursorX,
            _dragWindowY + pt.Y - _dragCursorY));
    }

    private void DragHandle_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        _isDragging = false;
        (sender as UIElement)?.ReleasePointerCapture(e.Pointer);
    }

    private void DragHandle_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        => _isDragging = false;

    // +--------------------------------------------------+
    // |           BUTTON HOVER ANIMATION + SOUND         |
    // +--------------------------------------------------+

    private static void ScaleWidgetBtn(UIElement element, float target)
    {
        var visual     = ElementCompositionPreview.GetElementVisual(element);
        var compositor = visual.Compositor;

        if (element is FrameworkElement fe && fe.ActualWidth > 0)
            visual.CenterPoint = new Vector3(
                (float)fe.ActualWidth  / 2f,
                (float)fe.ActualHeight / 2f, 0f);

        var ease  = compositor.CreateCubicBezierEasingFunction(
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

    private void WidgetBtn_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        ElementSoundPlayer.Play(ElementSoundKind.Focus);
        if (sender is UIElement el) ScaleWidgetBtn(el, 1.12f);
    }

    private void WidgetBtn_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is UIElement el) ScaleWidgetBtn(el, 1.0f);
    }

    // +--------------------------------------------------+
    // |                 BUTTON HANDLERS                  |
    // +--------------------------------------------------+

    private void PinBtn_Click(object sender, RoutedEventArgs e)
    {
        _isPinned = !_isPinned;
        ApplyTopmost();
        UpdatePinButton();

        var cfg = _settings.Load();
        cfg.WidgetAlwaysOnTop = _isPinned;
        _settings.Save(cfg);
    }

    private void SizeBtn_Click(object sender, RoutedEventArgs e)
    {
        _isLarge = !_isLarge;

        _appWindow.Resize(new SizeInt32(
            _isLarge ? LargeWidth  : SmallWidth,
            _isLarge ? LargeHeight : SmallHeight));

        ShowCurrentView();

        var cfg = _settings.Load();
        cfg.WidgetSize = _isLarge ? "Large" : "Small";
        _settings.Save(cfg);
    }

    private void MuteBtn_Click(object sender, RoutedEventArgs e)
    {
        _isSoundEnabled = !_isSoundEnabled;
        ApplySoundState();

        var cfg = _settings.Load();
        cfg.SoundEnabled = _isSoundEnabled;
        _settings.Save(cfg);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        var fade = new DoubleAnimation
        {
            To             = 0,
            Duration       = new Duration(TimeSpan.FromMilliseconds(150)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn },
        };
        Storyboard.SetTarget(fade, RootBorder);
        Storyboard.SetTargetProperty(fade, "Opacity");

        var sb = new Storyboard();
        sb.Children.Add(fade);
        sb.Completed += (_, _) => this.Close();
        sb.Begin();
    }

    // +--------------------------------------------------+
    // |                    CLEANUP                       |
    // +--------------------------------------------------+

    private void OnWidgetClosed()
    {
        _appWindow.Changed -= AppWindow_Changed;
        _widgetVm.Dispose();
        _acrylicController?.Dispose();
    }
}
