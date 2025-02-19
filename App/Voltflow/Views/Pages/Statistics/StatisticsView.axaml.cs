using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.Views.Pages.Statistics;

public partial class StatisticsView : ReactiveUserControl<StatisticsViewModel>
{
	public StatisticsView()
	{
		InitializeComponent();
	}
}