using ReactiveUI;
using Splat;
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

    // dependency injection stuff
    protected T GetService<T>() => Locator.Current.GetService<T>()!;
}
