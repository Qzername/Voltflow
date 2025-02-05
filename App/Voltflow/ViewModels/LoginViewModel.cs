using ReactiveUI;

namespace Voltflow.ViewModels;

public class LoginViewModel(IScreen screen) : ViewModelBase(screen)
{
	public void NavigateBack()
	{
		HostScreen.Router.NavigateBack.Execute();
	}
}

