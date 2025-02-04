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
			TestViewModel context => new TestView { DataContext = context },
			LoginViewModel context => new LoginView { DataContext = context },
			_ => throw new ArgumentOutOfRangeException(nameof(viewModel))
		};
	}
}
