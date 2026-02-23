using CommunityToolkit.Mvvm.ComponentModel;

namespace Calender.ViewModels;

/// <summary>
/// Shell-level view model — owns state shared across all pages
/// (e.g. active nav item, global date selection).
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedNavItem = "Calendar";

    [ObservableProperty]
    private DateTime _todayDate = DateTime.Today;
}
