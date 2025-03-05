using Avalonia;
using Avalonia.ReactiveUI;
using Avalonia.SimplePreferences;
using Avalonia.Styling;
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

        if (Application.Current != null)
        {
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
}