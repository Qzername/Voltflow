using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.SimplePreferences;
using Splat;
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

		var client = Locator.Current.GetService<HttpClient>();
		if (client is not null && token is not null)
			client.DefaultRequestHeaders.Authorization = new("Bearer", token);
	}
}