using Calender.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT;

namespace Calender;

public sealed partial class MainWindow : Window
{
    // Map navigation tags
    private static readonly Dictionary<string, Type> _pageMap = new()
    {
        ["CalendarPage"] = typeof(CalendarPage),
        ["AgendaPage"]   = typeof(AgendaPage),
    };

    // Nullable — null means the widget is not currently open
    private WidgetWindow? _widgetWindow;

    // Programmatic Mica backdrop (keeps IsInputActive = true so it never dims on blur)
    private MicaController? _micaController;

    public MainWindow()
    {
        this.InitializeComponent();
        Title = "Calender";
        SetupBackdrop();
        this.Closed += (_, _) => _micaController?.Dispose();
    }

    // ── Backdrop ──────────────────────────────────────────────────────────────

    private void SetupBackdrop()
    {
        if (!MicaController.IsSupported()) return;

        // IsInputActive = true → backdrop stays fully lit even when window loses focus
        var config = new SystemBackdropConfiguration { IsInputActive = true };

        _micaController = new MicaController { Kind = MicaKind.BaseAlt };
        _micaController.AddSystemBackdropTarget(
            this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        _micaController.SetSystemBackdropConfiguration(config);
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Select the first item and navigate to it on startup
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(CalendarPage));
    }

    private void NavView_SelectionChanged(NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected) return;

        if (args.SelectedItemContainer?.Tag is string tag
            && _pageMap.TryGetValue(tag, out var pageType))
        {
            ContentFrame.Navigate(pageType);
        }
    }

    // ── Widget toggle ─────────────────────────────────────────────────────────

    private void WidgetToggle_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (_widgetWindow is null)
        {
            _widgetWindow = new WidgetWindow();
            _widgetWindow.Closed += (_, _) => _widgetWindow = null;
            _widgetWindow.Activate();
        }
        else
        {
            _widgetWindow.Close();
            // _widgetWindow is set back to null by the Closed handler above
        }
    }
}
