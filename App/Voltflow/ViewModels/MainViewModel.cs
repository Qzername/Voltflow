using ReactiveUI;
using System.Reactive;

namespace Voltflow.ViewModels;

/// <summary>
/// ViewModel for MainView that acts as a router.
/// </summary>
public class MainViewModel : ReactiveObject, IScreen
{
	public RoutingState Router { get; } = new();
	public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

	/// <summary>
	/// Constructor for MainViewModel.
	/// When constructed, the router navigates to TestView by default.
	/// </summary>
	public MainViewModel()
	{
		Router.Navigate.Execute(new MapViewModel(this));
	}

	/// <summary>
	/// Currently, NavBar only has a "Sign In" button that navigates to AccountView.
	/// If user is already in AccountView, don't navigate again.
	/// </summary>
	public void NavigateToAccountView()
	{
		if (Router.GetCurrentViewModel()?.GetType() != typeof(AccountViewModel)) Router.Navigate.Execute(new AccountViewModel(this));
	}
}

