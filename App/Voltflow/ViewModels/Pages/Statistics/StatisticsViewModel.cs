using ReactiveUI;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel(IScreen screen) : ViewModelBase(screen)
{
	public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(screen));
}