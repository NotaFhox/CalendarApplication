using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    public SettingsPage()
    {
        this.InitializeComponent();
    }
}
