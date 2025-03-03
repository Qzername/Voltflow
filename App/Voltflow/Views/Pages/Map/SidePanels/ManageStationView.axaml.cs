using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Map.SidePanels;

namespace Voltflow.Views.Pages.Map.SidePanels;

public partial class ManageStationView : ReactiveUserControl<ManageStationViewModel>
{
    public ManageStationView()
    {
        InitializeComponent();
    }

    // https://github.com/irihitech/Ursa.Avalonia/blob/main/demo/Ursa.Demo/Pages/ToastDemo.axaml.cs
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is not ManageStationViewModel viewModel)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (DataContext is ManageStationViewModel viewModel)
            viewModel.ToastManager?.Uninstall();
    }
}