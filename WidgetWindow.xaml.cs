using System.Runtime.InteropServices;
using Calender.Services;
using Calender.ViewModels;
using Calender.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;

namespace Calender;

public sealed partial class WidgetWindow : Window
{
    // ── Win32 constants ────────────────────────────────────────────────────────

    private const uint WM_NCLBUTTONDOWN = 0x00A1;
    private const nint HTCAPTION        = 2;
    private const uint SWP_NOSIZE       = 0x0001;
    private const uint SWP_NOMOVE       = 0x0002;
    private const uint SWP_NOACTIVATE   = 0x0010;
    private static readonly nint HWND_TOPMOST = new(-1);

    // ── P/Invoke ───────────────────────────────────────────────────────────────

    [LibraryImport("user32.dll")]
    private static partial nint PostMessageW(nint hWnd, uint msg, nint wParam, nint lParam);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetWindowPos(
        nint hWnd, nint hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    // ── Fields ─────────────────────────────────────────────────────────────────

    private readonly WidgetViewModel _widgetVm = new();
    private readonly SettingsService _settings  = new();
    private readonly AppWindow       _appWindow;
    private readonly nint            _hwnd;

    private bool _isLarge = true;   // tracks current size mode

    // Widget dimensions (physical pixels at 100% DPI; Windows App SDK scales for us)
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

        ConfigureWindow();
        RestorePositionAndSize();
        ShowCurrentView();

        _appWindow.Changed += AppWindow_Changed;
        this.Closed        += (_, _) => OnWidgetClosed();
    }

    // ── Window configuration ───────────────────────────────────────────────────

    private void ConfigureWindow()
    {
        // Frameless — removes the OS title-bar and system border for a clean widget look
        if (_appWindow.Presenter is OverlappedPresenter p)
            p.SetBorderAndTitleBar(false, false);

        // Keep widget off Alt-Tab and the taskbar
        _appWindow.IsShownInSwitchers = false;

        // Always-on-top via SetWindowPos with HWND_TOPMOST; no size/move/activate side-effects
        SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0,
            SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
    }

    private void RestorePositionAndSize()
    {
        var cfg = _settings.Load();

        _isLarge = cfg.WidgetSize != "Small";

        // Guard against uninitialized / off-screen defaults
        int x = cfg.WidgetX > 0 ? cfg.WidgetX : 80;
        int y = cfg.WidgetY > 0 ? cfg.WidgetY : 80;
        int w = _isLarge ? LargeWidth  : SmallWidth;
        int h = _isLarge ? LargeHeight : SmallHeight;

        _appWindow.MoveAndResize(new RectInt32(x, y, w, h));
    }

    // ── View switching ─────────────────────────────────────────────────────────

    /// <summary>
    /// Loads the correct view into WidgetFrame and refreshes the size icon/tooltip.
    /// We set Content directly (not Frame.Navigate) so we can inject the ViewModel
    /// via constructor without requiring a default ctor + OnNavigatedTo boilerplate.
    /// </summary>
    private void ShowCurrentView()
    {
        WidgetFrame.Content = _isLarge
            ? (object)new LargeWidgetView(_widgetVm)
            : new SmallWidgetView(_widgetVm);

        // E73D = "Back to Window" (compress)  E73F = "FullScreen" (expand)
        SizeIcon.Glyph = _isLarge ? "" : "";
        ToolTipService.SetToolTip(SizeBtn, _isLarge ? "Compact view" : "Full view");
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

    private void RootBorder_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Hand the drag off to the OS: WM_NCLBUTTONDOWN with HTCAPTION causes Windows
        // to move the window in response to pointer movement, including Aero Snap.
        PostMessageW(_hwnd, WM_NCLBUTTONDOWN, HTCAPTION, 0);
    }

    private void SizeBtn_Click(object sender, RoutedEventArgs e)
    {
        _isLarge = !_isLarge;

        // Resize window first, then swap content to avoid a layout flash
        int w = _isLarge ? LargeWidth  : SmallWidth;
        int h = _isLarge ? LargeHeight : SmallHeight;
        _appWindow.Resize(new SizeInt32(w, h));

        ShowCurrentView();

        // Persist the new size preference
        var cfg = _settings.Load();
        cfg.WidgetSize = _isLarge ? "Large" : "Small";
        _settings.Save(cfg);
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => this.Close();

    // ── Cleanup ────────────────────────────────────────────────────────────────

    private void OnWidgetClosed()
    {
        _appWindow.Changed -= AppWindow_Changed;
        _widgetVm.Dispose();
    }
}
