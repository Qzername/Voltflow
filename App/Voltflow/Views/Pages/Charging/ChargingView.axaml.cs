using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.Views.Pages.Charging;

public partial class ChargingView : ReactiveUserControl<ChargingViewModel>
{
	public ChargingView()
	{
		InitializeComponent();
	}

	// https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		if (DataContext is not ChargingViewModel viewModel)
			return;

		var topLevel = TopLevel.GetTopLevel(this);
		viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
	}

	protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnDetachedFromVisualTree(e);
		if (DataContext is ChargingViewModel viewModel)
			viewModel.ToastManager?.Uninstall();
	}
}