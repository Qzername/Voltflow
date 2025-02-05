using Avalonia.ReactiveUI;
using System;
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

	private void TestView_DataContextChanged(object? sender, EventArgs e)
	{
		var viewModel = DataContext as TestViewModel;
		viewModel!.ConfigureMap();
		MapControl.Map = viewModel.Map!;
	}
}
