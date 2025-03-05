using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
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

        if (viewModel.NoCars)
            viewModel.ToastManager.Show(
                new Toast("Your account has no cars added. Create one by filling the form below."),
                showIcon: true,
                showClose: false,
                type: NotificationType.Warning,
                classes: ["Light"]);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is AddCarViewModel viewModel)
            viewModel.ToastManager?.Uninstall();
    }
}