using Avalonia.Controls;
using Mapsui;
using Mapsui.UI;
using Mapsui.UI.Avalonia;
using System;
using System.Diagnostics;

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

        var mpoint = new MPoint(screenPosition.X, screenPosition.Y);

        var minDistance = 50; // Pixels
        var feature = map.GetMapInfo(mpoint, minDistance)?.Feature;

        Debug.WriteLine("-----------------");
        Debug.WriteLine("Touch");
        Debug.WriteLine("F " + feature is not null);
        Debug.WriteLine("-----------------");
    }

    private void MainView_DataContextChanged(object? sender, EventArgs e)
    {
        var viewModel = DataContext as ViewModels.MainViewModel;
        viewModel!.ConfigureMap();

        map.Map = viewModel.Map;
    }
}
