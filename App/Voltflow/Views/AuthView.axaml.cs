using Avalonia.ReactiveUI;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class AuthView : ReactiveUserControl<AuthViewModel>
{
	public AuthView()
	{
		InitializeComponent();
	}
}