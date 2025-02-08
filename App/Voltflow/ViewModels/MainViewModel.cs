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
		Router.Navigate.Execute(new TestViewModel(this));
	}

	/// <summary>
	/// Currently, NavBar only has a "Sign In" button that navigates to AuthView.
	/// If user is already in AuthView, don't navigate again.
	/// </summary>
	public void NavigateToAuthView()
	{
		if (Router.GetCurrentViewModel()?.GetType() != typeof(AuthViewModel)) Router.Navigate.Execute(new AuthViewModel(this));
	}
}

