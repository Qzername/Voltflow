using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Reactive;
using Voltflow.Models;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class MapView : ReactiveUserControl<MapViewModel>
{
	public MapView()
	{
		InitializeComponent();

		/*
         * View's constructor is called faster than viewmodel constructor
         * therefore I need to wait for the DataContext change and call map configuration
         * 
         * I as well cannot bind map directly to the viewmodel
         * because this library for some reason does not support this 
        */
		DataContextChanged += TestView_DataContextChanged;
		MapControl.Tapped += MapControl_Tapped;

		// X = set amount of pixels in Display.cs.
		// Checks viewport width. If it's less than or equal X, set CurrentDisplayMode to Mobile - THIS WILL HIDE SOME ELEMENTS IN THE ADMIN PANEL!
		this.GetObservable(BoundsProperty).Subscribe(Observer.Create<Rect>(bounds =>
		{
			if (DataContext is not MapViewModel viewModel) return;
			viewModel.CurrentDisplayMode = bounds.Width > Display.MaxMobileWidth ? DisplayMode.Desktop : DisplayMode.Mobile;
		}));
	}

	private void MapControl_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
	{
		/*
         * Another bad thing about this library - and this is a fix for it
         * When you create a new point Mapsui does not update the view
         * So the point is invisible until you move the map
         * Refresh(), RefreshGraphics(), RefreshData() - neither of these works
         * But InvalidateVisual() does the job
         */
		MapControl.InvalidateVisual();
	}

	private async void TestView_DataContextChanged(object? sender, EventArgs e)
	{
		var viewModel = DataContext as MapViewModel;
		await viewModel!.ConfigureMap();
		MapControl.Map = viewModel.Map!;
	}
}
