using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;
using Voltflow.Models;

namespace Voltflow.ViewModels;

public class AuthViewModel(IScreen screen) : ViewModelBase(screen)
{
	[Reactive] public AuthType AuthType { get; set; } = AuthType.SignIn;
	[Reactive] public bool Authenticating { get; set; }

	public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new TestViewModel(screen));

	public void NavigateBack()
	{
		AuthType = AuthType.SignIn;
	}

	public void SwitchForms() => AuthType = AuthType == AuthType.SignIn ? AuthType.SignUp : AuthType.SignIn;

	// This function just tests switching between forms
	// It will be deleted in next commits
	public async void TestAuthentication()
	{
		Authenticating = true;
		await Task.Delay(1000);
		Authenticating = false;
		AuthType = AuthType == AuthType.TwoFactor ? AuthType.SignIn : AuthType.TwoFactor;
	}
}

