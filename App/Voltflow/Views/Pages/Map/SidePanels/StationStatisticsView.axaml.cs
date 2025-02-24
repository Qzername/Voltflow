using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Map.SidePanels;

namespace Voltflow.Views.Pages.Map.SidePanels;

public partial class StationStatisticsView : ReactiveUserControl<StationStatisticsViewModel>
{
    public StationStatisticsView()
    {
        InitializeComponent();
    }
}