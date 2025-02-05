using Avalonia.ReactiveUI;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class LoginView : ReactiveUserControl<LoginViewModel>
{
	public LoginView()
	{
		InitializeComponent();
	}
}