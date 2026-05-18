using Calender.Views;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinRT;

namespace Calender;

public sealed partial class MainWindow : Window
{
    // +--------------------------------------------------+
    // |                    FIELDS                        |
    // +--------------------------------------------------+

    private static readonly Dictionary<string, Type> _pages = new()
    {
        ["CalendarPage"] = typeof(CalendarPage),
        ["AgendaPage"]   = typeof(AgendaPage),
    };

    private WidgetWindow?    _widgetWindow;
    private MicaController?  _micaController;

    // +--------------------------------------------------+
    // |                  CONSTRUCTION                    |
    // +--------------------------------------------------+

    public MainWindow()
    {
        this.InitializeComponent();
        Title = "Calender";
        SetupBackdrop();
        this.Closed += (_, _) => _micaController?.Dispose();
    }

    // +--------------------------------------------------+
    // |                    BACKDROP                      |
    // +--------------------------------------------------+

    private void SetupBackdrop()
    {
        if (!MicaController.IsSupported()) return;

        var config = new SystemBackdropConfiguration { IsInputActive = true };

        _micaController = new MicaController { Kind = MicaKind.BaseAlt };
        _micaController.AddSystemBackdropTarget(
            this.As<Microsoft.UI.Composition.ICompositionSupportsSystemBackdrop>());
        _micaController.SetSystemBackdropConfiguration(config);
    }

    // +--------------------------------------------------+
    // |                   NAVIGATION                     |
    // +--------------------------------------------------+

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(CalendarPage));
    }

    private void NavView_SelectionChanged(NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItemContainer?.Tag is string tag
            && _pages.TryGetValue(tag, out var pageType))
        {
            ContentFrame.Navigate(pageType);
        }
    }

    // +--------------------------------------------------+
    // |                 WIDGET TOGGLE                    |
    // +--------------------------------------------------+

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
        }
    }
}
