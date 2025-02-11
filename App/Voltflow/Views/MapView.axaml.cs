using Avalonia.ReactiveUI;
using System;
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
	}

	private void TestView_DataContextChanged(object? sender, EventArgs e)
	{
		var viewModel = DataContext as MapViewModel;
		viewModel!.ConfigureMap();
		MapControl.Map = viewModel.Map!;
	}
}
