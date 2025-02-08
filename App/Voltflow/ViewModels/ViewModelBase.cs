using ReactiveUI;
using System;

namespace Voltflow.ViewModels;

/// <summary>
/// Base for all ViewModels.
/// </summary>
/// <param name="screen"></param>
public class ViewModelBase(IScreen screen) : ReactiveObject, IRoutableViewModel
{
	public IScreen HostScreen { get; } = screen;
	public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
}
