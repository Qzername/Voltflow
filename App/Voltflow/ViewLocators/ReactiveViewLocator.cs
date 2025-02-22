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

		if (type is null)
			throw new Exception($"Did not found view with name {name}\nIs the view in correct namespace?");

		//!!!write more detailed exception message when you encounter this error!!!
		if (Activator.CreateInstance(type) is null)
			throw new Exception($"View not found for {viewModel.GetType().FullName}");

		return (IViewFor)Activator.CreateInstance(type)!;
	}
}
