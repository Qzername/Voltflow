using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.Views.Pages.Charging;

public partial class ChargingView : ReactiveUserControl<ChargingViewModel>
{
    public ChargingView()
    {
        InitializeComponent();
    }
}