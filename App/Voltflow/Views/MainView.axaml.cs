using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.SimplePreferences;
using Avalonia.Styling;
using Splat;
using System.Net;
using System.Net.Http;
using Voltflow.ViewModels;

namespace Voltflow.Views;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
	public MainView()
	{
		InitializeComponent();
	}

	protected async override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
	{
		base.OnAttachedToVisualTree(e);

		if (DataContext is not MainViewModel viewModel)
			return;

		var token = await Preferences.GetAsync<string>("token", null);
		viewModel.Authenticated = token != null;

		var httpClient = Locator.Current.GetService<HttpClient>();
		if (httpClient is not null && token is not null)
		{
			httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);

			// Check if token is valid, account deleted, etc.
			var request = await httpClient.GetAsync("/api/Accounts");

			if (request.StatusCode != HttpStatusCode.OK)
			{
				await Preferences.RemoveAsync("token");
				httpClient.DefaultRequestHeaders.Authorization = null;
				viewModel.Authenticated = false;
			}

			// Checks if user is admin.
			request = await httpClient.GetAsync("/api/Accounts/adminCheck");
			viewModel.IsAdmin = request.StatusCode == HttpStatusCode.OK;
		}

		if (Application.Current == null)
			return;

		var theme = await Preferences.GetAsync<string?>("theme", null);
		switch (theme)
		{
			case "dark":
				Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
				break;
			case "light":
				Application.Current.RequestedThemeVariant = ThemeVariant.Light;
				break;
			default: // This is used when theme is null or "default".
				Application.Current.RequestedThemeVariant = ThemeVariant.Default;
				break;
		}
	}
}