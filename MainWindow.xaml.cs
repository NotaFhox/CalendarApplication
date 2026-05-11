using Calender.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Calender;

public sealed partial class MainWindow : Window
{
    // Map navigation tags
    private static readonly Dictionary<string, Type> _pageMap = new()
    {
        ["CalendarPage"] = typeof(CalendarPage),
        ["AgendaPage"]   = typeof(CalendarPage), // placeholder until AgendaPage is created
    };

    // Nullable — null means the widget is not currently open
    private WidgetWindow? _widgetWindow;

    public MainWindow()
    {
        this.InitializeComponent();
        Title = "Calender";
    }

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
