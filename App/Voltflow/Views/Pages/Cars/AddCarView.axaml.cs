using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Cars;

namespace Voltflow.Views.Pages.Cars;

public partial class AddCarView : ReactiveUserControl<AddCarViewModel>
{
	public AddCarView()
	{
		InitializeComponent();
	}

	// https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		if (DataContext is not AddCarViewModel viewModel)
			return;

		var topLevel = TopLevel.GetTopLevel(this);
		viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		if (DataContext is AddCarViewModel viewModel)
			viewModel.ToastManager?.Uninstall();
	}
}