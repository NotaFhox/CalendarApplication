using Calender.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

public sealed partial class LargeWidgetView : Page
{
    public WidgetViewModel ViewModel { get; }

    public LargeWidgetView(WidgetViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}
