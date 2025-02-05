using Avalonia.ReactiveUI;
using Mapsui;
using System;
using System.Diagnostics;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class TestView : ReactiveUserControl<TestViewModel>
{
	public TestView()
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

		/* map.Tapped += Map_Tapped;
		 map.DoubleTapped += Map_DoubleTapped;*/
	}

	private void Map_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
	{
		Debug.WriteLine("-----------------");
		Debug.WriteLine("DT");
		Debug.WriteLine("-----------------");
	}

	private void Map_Tapped(object? sender, Avalonia.Input.TappedEventArgs e)
	{
		var screenPosition = e.GetPosition(map);

		var mPoint = new MPoint(screenPosition.X, screenPosition.Y);

		var minDistance = 50; // Pixels
		var feature = map.GetMapInfo(mPoint, minDistance)?.Feature;

		Debug.WriteLine("-----------------");
		Debug.WriteLine("Touch");
		Debug.WriteLine("F " + feature is not null);
		Debug.WriteLine("-----------------");
	}

	private void TestView_DataContextChanged(object? sender, EventArgs e)
	{
		var viewModel = DataContext as TestViewModel;
		viewModel!.ConfigureMap();
		map.Map = viewModel.Map;
	}
}
