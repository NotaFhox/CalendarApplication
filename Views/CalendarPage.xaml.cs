using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class CalendarPage : Page
{
       public CalendarViewModel ViewModel { get; } = new();

    public CalendarPage()
    {
        this.InitializeComponent();
    }
}
