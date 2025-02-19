using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Cars;

namespace Voltflow.Views.Pages.Cars;

public partial class AddCarView : ReactiveUserControl<AddCarViewModel>
{
    public AddCarView()
    {
        InitializeComponent();
    }
}