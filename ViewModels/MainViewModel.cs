using CommunityToolkit.Mvvm.ComponentModel;

namespace Calender.ViewModels;

/// <summary>
/// Shell-level view model — owns state shared across the navigation shell.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private string   _selectedNavItem = "Calendar";
    [ObservableProperty] private DateTime _todayDate       = DateTime.Today;
}
