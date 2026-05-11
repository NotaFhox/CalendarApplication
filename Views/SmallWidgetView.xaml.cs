using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class SmallWidgetView : Page
{
    public WidgetViewModel ViewModel { get; }

    public SmallWidgetView(WidgetViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}
