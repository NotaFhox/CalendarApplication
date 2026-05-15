using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class AgendaPage : Page
{
    public AgendaViewModel ViewModel { get; } = new();

    public AgendaPage()
    {
        this.InitializeComponent();
    }
}
