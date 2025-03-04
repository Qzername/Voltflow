using ReactiveUI;
using Splat;
using System;
using System.Reactive;

namespace Voltflow.ViewModels;

/// <summary>
/// Base for all ViewModels.
/// </summary>
/// <param name="screen"></param>
public class ViewModelBase(IScreen screen) : ReactiveObject, IRoutableViewModel
{
    public IScreen HostScreen { get; } = screen;
    public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;

    // dependency injection stuff
    protected T GetService<T>() => Locator.Current.GetService<T>()!;
}
