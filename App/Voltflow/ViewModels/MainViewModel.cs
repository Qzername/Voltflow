using System.Diagnostics;
using ReactiveUI;
using System.Reactive;

namespace Voltflow.ViewModels;

public class MainViewModel : ReactiveObject, IScreen
{
	public RoutingState Router { get; } = new RoutingState();
	// public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

	public MainViewModel()
	{
		Router.Navigate.Execute(new TestViewModel(this));
	}

	public void RouteToLogin()
	{
		if (Router.GetCurrentViewModel()?.GetType() != typeof(AuthViewModel)) Router.Navigate.Execute(new AuthViewModel(this));
	}
}

