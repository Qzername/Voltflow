using ReactiveUI;

namespace Voltflow.ViewModels;

enum AuthType
{
	LogIn = 0,
	Register = 1
}

public class AuthViewModel(IScreen screen) : ViewModelBase(screen)
{
	public void NavigateBack()
	{
		HostScreen.Router.NavigateBack.Execute();
	}
}

