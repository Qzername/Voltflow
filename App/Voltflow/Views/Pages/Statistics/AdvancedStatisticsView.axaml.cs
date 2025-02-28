using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.Views.Pages.Statistics;

public partial class AdvancedStatisticsView : ReactiveUserControl<AdvancedStatisticsViewModel>
{
	public AdvancedStatisticsView()
	{
		InitializeComponent();
	}

	protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		if (DataContext is AdvancedStatisticsViewModel viewModel)
		{
			viewModel.Parent = this;

			var topLevel = TopLevel.GetTopLevel(this);
			viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
		}
	}
}