using Calender.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Calender.Views;

/// <summary>
/// Picks the correct DataTemplate for the Agenda flat list:
/// <see cref="AgendaHeader"/> rows → HeaderTemplate;
/// CalendarEvent rows → EventTemplate.
/// </summary>
public sealed class AgendaTemplateSelector : DataTemplateSelector
{
    public DataTemplate? HeaderTemplate { get; set; }
    public DataTemplate? EventTemplate  { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
        => item is AgendaHeader ? HeaderTemplate! : EventTemplate!;
}
