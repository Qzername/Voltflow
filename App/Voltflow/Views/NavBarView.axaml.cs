using Avalonia.ReactiveUI;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class NavBarView : ReactiveUserControl<MainViewModel>
{
	public NavBarView()
	{
		InitializeComponent();
	}
}