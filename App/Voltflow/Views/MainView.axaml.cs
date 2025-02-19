using Avalonia;
using Avalonia.ReactiveUI;
using System.Reactive;
using Voltflow.Models;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
	public MainView()
	{
		InitializeComponent();

		// X = set amount of pixels in Display.cs.
		// Checks viewport width. If it's less than or equal X, set CurrentDisplayMode to Mobile - THIS WILL HIDE SOME ELEMENTS IN THE ADMIN PANEL!
		this.GetObservable(BoundsProperty).Subscribe(Observer.Create<Rect>(bounds =>
		{
			if (DataContext is not MainViewModel viewModel) return;
			viewModel.CurrentDisplayMode = bounds.Width > Display.MaxMobileWidth ? DisplayMode.Desktop : DisplayMode.Mobile;
		}));
	}
}