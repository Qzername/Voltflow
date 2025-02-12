using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class AuthView : ReactiveUserControl<AuthViewModel>
{
	public AuthView()
	{
		InitializeComponent();
	}

	// https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);
		var topLevel = TopLevel.GetTopLevel(this);
		if (DataContext is AuthViewModel viewModel)
			viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		if (DataContext is AuthViewModel viewModel)
			viewModel.ToastManager?.Uninstall();
	}
}