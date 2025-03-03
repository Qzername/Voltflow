using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Cars;

namespace Voltflow.Views.Pages.Cars;

public partial class CarsView : ReactiveUserControl<CarsViewModel>
{
    public CarsView()
    {
        InitializeComponent();
    }
}