using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.Views.Pages.Charging;

public partial class TransactionView : ReactiveUserControl<TransactionViewModel>
{
    public TransactionView()
    {
        InitializeComponent();
    }

    // https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is not TransactionViewModel viewModel)
            return;

        viewModel.Parent = this;

        var topLevel = TopLevel.GetTopLevel(this);
        viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is TransactionViewModel viewModel)
            viewModel.ToastManager?.Uninstall();
    }
}