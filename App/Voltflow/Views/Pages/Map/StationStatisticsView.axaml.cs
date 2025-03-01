using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.Views.Pages.Map;

public partial class StationStatisticsView : ReactiveUserControl<StationStatisticsViewModel>
{
	public StationStatisticsView()
	{
		InitializeComponent();
	}
}