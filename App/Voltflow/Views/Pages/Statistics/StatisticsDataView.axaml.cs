using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.Views.Pages.Statistics;

public partial class StatisticsDataView : ReactiveUserControl<StatisticsDataViewModel>
{
    public StatisticsDataView()
    {
        InitializeComponent();
    }
}