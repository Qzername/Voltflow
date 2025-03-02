using and = Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia;
using Avalonia.Android;
using Avalonia.ReactiveUI;

namespace Voltflow.Android;

[Activity(
	Label = "Voltflow",
	Theme = "@style/Voltflow",
	Icon = "@mipmap/ic_launcher",
    RoundIcon = "@mipmap/ic_launcher",
	MainLauncher = true,
	ScreenOrientation = ScreenOrientation.Portrait,
	ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // API 33 = Android 13
            if (ContextCompat.CheckSelfPermission(this, and.Manifest.Permission.PostNotifications) != Permission.Granted)
                ActivityCompat.RequestPermissions(this, [and.Manifest.Permission.PostNotifications], 0);

        var notificationService = new AndroidNotificationService(this);
        App.NotificationService = notificationService;
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
	{
		return base.CustomizeAppBuilder(builder)
			.WithFont_Roboto()
			.UseReactiveUI();
	}
}
