using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Cars;

namespace Voltflow.Views.Pages.Cars;

public partial class CarInfoView : ReactiveUserControl<CarInfoViewModel>
{
	public CarInfoView()
	{
		InitializeComponent();
	}
}