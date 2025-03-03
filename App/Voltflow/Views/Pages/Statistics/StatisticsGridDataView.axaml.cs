using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.Views.Pages.Statistics;

public partial class StatisticsGridDataView : ReactiveUserControl<StatisticsGridDataViewModel>
{
    public StatisticsGridDataView()
    {
        InitializeComponent();
    }
}