using Avalonia.Controls;
using Avalonia;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Charging;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.Views.Pages.Charging;

public partial class TransactionView : ReactiveUserControl<TransactionViewModel>
{
	public TransactionView()
	{
		InitializeComponent();
	}
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is TransactionViewModel viewModel)
        {
            viewModel.Parent = this;
        }
    }
}