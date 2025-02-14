using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HotAvalonia;
using Splat;
using Voltflow.ViewModels;
using Voltflow.Views;

namespace Voltflow;

public partial class App : Application
{
	public override void Initialize()
	{
		string currentDirectory = Directory.GetCurrentDirectory();
		if (!File.Exists($"{currentDirectory}/settings.json"))
			File.Create($"{currentDirectory}/settings.json");

		if (!(ApplicationLifetime is ISingleViewApplicationLifetime))
			this.EnableHotReload();
		AvaloniaXamlLoader.Load(this);
	}

	public override void OnFrameworkInitializationCompleted()
	{
		// Line below is needed to remove Avalonia data validation.
		// Without this line you will get duplicate validations from both Avalonia and CT
		BindingPlugins.DataValidators.RemoveAt(0);

		Bootstraper.Register(Locator.CurrentMutable, Locator.Current);

		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			desktop.MainWindow = new MainWindow
			{
				DataContext = new MainViewModel()
			};
		}
		else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
		{
			singleViewPlatform.MainView = new MainView
			{
				DataContext = new MainViewModel()
			};
		}

		base.OnFrameworkInitializationCompleted();
	}
}
