using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Account;

namespace Voltflow.Views.Account;

public partial class ChangePasswordView : ReactiveUserControl<ChangePasswordViewModel>
{
    public ChangePasswordView()
    {
        InitializeComponent();
    }

    // https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is not ChangePasswordViewModel viewModel)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is ChangePasswordViewModel viewModel)
            viewModel.ToastManager?.Uninstall();
    }
}