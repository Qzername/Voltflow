using Android.App;
using Android.Content.PM;

using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace Voltflow.Android;

[Activity(
	Label = "Voltflow",
	Theme = "@style/MyTheme.NoActionBar",
	Icon = "@drawable/icon",
	MainLauncher = true,
	ScreenOrientation = ScreenOrientation.Portrait,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
	protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
	{
		return base.CustomizeAppBuilder(builder)
			.WithInterFont()
			.UseReactiveUI();
	}
}
