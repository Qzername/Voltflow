using Avalonia.Controls;
using ReactiveUI;
using System;

namespace Voltflow.ViewLocators;

/// <summary>
///  This viewlocator tells ReactiveUI library which view to use for a given viewmodel.
///  
/// if i set the datacontext of a control to a viewmodel,
/// then this viewlocator will find the view for that viewmodel.
/// </summary>
public class ReactiveViewLocator : IViewLocator
{
    IViewFor IViewLocator.ResolveView<T>(T viewModel, string contract)
    {
        var name = viewModel!.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        return (IViewFor)Activator.CreateInstance(type!)!;
    }
}
