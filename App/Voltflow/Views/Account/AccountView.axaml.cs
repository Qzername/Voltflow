using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.ViewModels;
using Voltflow.ViewModels.Account;

namespace Voltflow.Views.Account;

public partial class AccountView : ReactiveUserControl<AccountViewModel>
{
    public AccountView()
    {
        InitializeComponent();
    }

    // https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is not AccountViewModel viewModel)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };

        if (viewModel.HostScreen is MainViewModel mainViewModel)
            viewModel.CurrentAuthType = mainViewModel.Authenticated ? AuthType.SignedIn : AuthType.SignIn;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is AccountViewModel viewModel)
            viewModel.ToastManager?.Uninstall();
    }
}