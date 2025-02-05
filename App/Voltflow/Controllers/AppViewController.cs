using ReactiveUI;
using System;
using Voltflow.ViewModels;
using Voltflow.Views;

namespace Voltflow.Controllers
{
	public class AppViewController : IViewLocator
	{
		public IViewFor ResolveView<T>(T? viewModel, string? contract = null) => viewModel switch
		{
			MainViewModel context => new MainView { DataContext = context },
			TestViewModel context => new TestView { DataContext = context },
			AuthViewModel context => new AuthView { DataContext = context },
			_ => throw new ArgumentOutOfRangeException(nameof(viewModel))
		};
	}
}
