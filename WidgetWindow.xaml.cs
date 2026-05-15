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
    // ── Win32 constants ────────────────────────────────────────────────────────

    private const uint WM_NCLBUTTONDOWN  = 0x00A1;
    private const nint HTCAPTION         = 2;
    private const uint SWP_NOSIZE        = 0x0001;
    private const uint SWP_NOMOVE        = 0x0002;
    private const uint SWP_NOACTIVATE    = 0x0010;
    private static readonly nint HWND_TOPMOST    = new(-1);
    private static readonly nint HWND_NOTOPMOST  = new(-2);

    // ── P/Invoke ───────────────────────────────────────────────────────────────

    [LibraryImport("user32.dll")]
    private static partial nint PostMessageW(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(
        nint hWnd, nint hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    // ── Fields ─────────────────────────────────────────────────────────────────

    private readonly WidgetViewModel        _widgetVm      = new();
    private readonly SettingsService        _settings      = new();
    private readonly AppWindow              _appWindow;
    private readonly nint                   _hwnd;
    private          DesktopAcrylicController? _acrylicController;

    private bool _isLarge  = true;   // tracks current size mode
    private bool _isPinned = true;   // tracks always-on-top state

    // Widget dimensions (logical pixels — Windows App SDK scales for DPI automatically)
    private const int LargeWidth  = 340;
    private const int LargeHeight = 420;
    private const int SmallWidth  = 220;
    private const int SmallHeight = 300;

    // ── Constructor ────────────────────────────────────────────────────────────

    public WidgetWindow()
    {
        this.InitializeComponent();

        _hwnd      = WinRT.Interop.WindowNative.GetWindowHandle(this);
        _appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(_hwnd));

        SetupBackdrop();       // must happen before ConfigureWindow
        ConfigureWindow();
        RestorePositionAndSize();
        ShowCurrentView();

        _appWindow.Changed += AppWindow_Changed;
        this.Closed        += (_, _) => OnWidgetClosed();
    }

    // ── Backdrop ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Programmatic acrylic so we can lock IsInputActive = true — the backdrop
    /// stays fully visible even when the widget does not have focus.
    /// </summary>
    private void SetupBackdrop()
    {
        if (!DesktopAcrylicController.IsSupported()) return;

        var config = new SystemBackdropConfiguration { IsInputActive = true };

        _acrylicController = new DesktopAcrylicController();
        _acrylicController.AddSystemBackdropTarget(
            this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        _acrylicController.SetSystemBackdropConfiguration(config);
    }

    // ── Window configuration ───────────────────────────────────────────────────

    private void ConfigureWindow()
    {
        // Frameless — removes the OS title-bar and border for a clean floating widget
        if (_appWindow.Presenter is OverlappedPresenter p)
            p.SetBorderAndTitleBar(false, false);

        // Keep widget out of Alt-Tab and the taskbar
        _appWindow.IsShownInSwitchers = false;
    }

    private void RestorePositionAndSize()
    {
        var cfg = _settings.Load();

        _isLarge  = cfg.WidgetSize != "Small";
        _isPinned = cfg.WidgetAlwaysOnTop;

        int x = cfg.WidgetX > 0 ? cfg.WidgetX : 80;
        int y = cfg.WidgetY > 0 ? cfg.WidgetY : 80;
        int w = _isLarge ? LargeWidth  : SmallWidth;
        int h = _isLarge ? LargeHeight : SmallHeight;

        _appWindow.MoveAndResize(new RectInt32(x, y, w, h));
        ApplyTopmost();
    }

    /// <summary>Applies or removes always-on-top based on _isPinned.</summary>
    private void ApplyTopmost()
    {
        var insertAfter = _isPinned ? HWND_TOPMOST : HWND_NOTOPMOST;
        SetWindowPos(_hwnd, insertAfter, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
    }

    // ── View switching ─────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the correct view into WidgetFrame and refreshes the control icons/tooltips.
    /// Content is set directly (not Frame.Navigate) to inject the shared ViewModel via constructor.
    /// </summary>
    private void ShowCurrentView()
    {
        WidgetFrame.Content = _isLarge
            ? (object)new LargeWidgetView(_widgetVm)
            : new SmallWidgetView(_widgetVm);

        // E73D = compress icon  E73F = expand icon
        SizeIcon.Glyph = _isLarge ? "" : "";
        ToolTipService.SetToolTip(SizeBtn, _isLarge ? "Compact view" : "Full view");

        UpdatePinButton();
    }

    private void UpdatePinButton()
    {
        // E718 = Pin (always on top active)   E77A = Unpin
        PinIcon.Glyph = _isPinned ? "" : "";
        PinIcon.Opacity = _isPinned ? 1.0 : 0.45;
        ToolTipService.SetToolTip(PinBtn,
            _isPinned ? "Unpin (allow behind other windows)" : "Pin (always on top)");
    }

    // ── Position persistence ───────────────────────────────────────────────────

    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidPositionChange) return;

        var pos = _appWindow.Position;
        var cfg = _settings.Load();
        cfg.WidgetX = pos.X;
        cfg.WidgetY = pos.Y;
        _settings.Save(cfg);
    }

    // ── XAML event handlers ────────────────────────────────────────────────────

    private void RootBorder_Loaded(object sender, RoutedEventArgs e)
    {
        // Fade the widget in from invisible on first load
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

    // Drag handle — only the header strip, not the content area
    private void DragHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // WM_NCLBUTTONDOWN + HTCAPTION tells Windows to move the window,
        // giving us free Aero Snap and smooth pointer tracking at no cost.
        PostMessageW(_hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
    }

    // ── Widget button hover animation + sound ──────────────────────────────────

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

        // Resize window first, then swap content to avoid a layout flash
        _appWindow.Resize(new SizeInt32(
            _isLarge ? LargeWidth  : SmallWidth,
            _isLarge ? LargeHeight : SmallHeight));

        ShowCurrentView();

        var cfg = _settings.Load();
        cfg.WidgetSize = _isLarge ? "Large" : "Small";
        _settings.Save(cfg);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        // Fade out before closing so the dismiss feels polished
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

    // ── Cleanup ────────────────────────────────────────────────────────────────

    private void OnWidgetClosed()
    {
        _appWindow.Changed -= AppWindow_Changed;
        _widgetVm.Dispose();
        _acrylicController?.Dispose();
    }
}
