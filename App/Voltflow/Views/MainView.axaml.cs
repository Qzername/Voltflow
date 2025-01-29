using Avalonia.Controls;
using Mapsui;
using System;

namespace Voltflow.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        /*
         * View's constructor is called faster than viewmodel constructor
         * therefore i need to wait for the datacontext change and call map configuration
         * 
         * I as well cannot bind map directly to the viewmodel
         * because this library for some reason does not support this 
         */
        DataContextChanged += MainView_DataContextChanged;
    }
    
    private void MainView_DataContextChanged(object? sender, EventArgs e)
    {
        var viewModel = DataContext as ViewModels.MainViewModel;
        viewModel!.ConfigureMap();

        map.Map = viewModel.Map;
    }
}
