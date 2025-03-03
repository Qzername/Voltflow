using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls.Notifications;
using Avalonia.SimplePreferences;
using Avalonia.Styling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Models.Forms;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Account;

public class SettingsViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;

    public WindowToastManager? ToastManager;
    public SettingsForm SettingsForm { get; set; }

    private readonly HttpClient _httpClient;
    private bool _clickedDelete;

    [Reactive] public AvaloniaList<ThemeVariant> Themes { get; set; } = [ThemeVariant.Default, ThemeVariant.Dark, ThemeVariant.Light];
    [Reactive] public ThemeVariant SelectedTheme { get; set; }

    public SettingsViewModel(IScreen screen, SettingsForm settingsForm, ThemeVariant? selectedTheme) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        SettingsForm = settingsForm;
        SelectedTheme = selectedTheme ?? ThemeVariant.Default;
    }

    public async Task SaveChanges()
    {
        bool nameValid = SettingsForm.Name == null || NameValidator.IsValid(SettingsForm.Name);
        bool surnameValid = SettingsForm.Surname == null || NameValidator.IsValid(SettingsForm.Surname);
        bool phoneNumberValid = SettingsForm.PhoneNumber == null || NumberValidator.IsValid(SettingsForm.PhoneNumber, 9);

        if (!nameValid || !surnameValid || !phoneNumberValid)
        {
            ToastManager?.Show(
                new Toast("Provided credentials do not meet our rules!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);
            return;
        }

        SettingsForm.Working = true;

        var json = new
        {
            Name = SettingsForm.Name,
            Surname = SettingsForm.Surname,
            PhoneNumber = SettingsForm.PhoneNumber
        };

        var content = JsonConverter.ToStringContent(json);
        var request = await _httpClient.PatchAsync("/api/Accounts", content);
        var twoFactorRequest = await _httpClient.PostAsync($"/api/Identity/TwoFactor/{(SettingsForm.TwoFactor ? "enable" : "disable")}", null);

        if (request.StatusCode == HttpStatusCode.OK && twoFactorRequest.StatusCode == HttpStatusCode.OK)
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeVariant = SelectedTheme;
                await Preferences.SetAsync("theme", ((string)SelectedTheme.Key).ToLower());
            }

            HostScreen.Router.NavigateAndReset.Execute(new AccountViewModel(HostScreen));
        }
        else
            ToastManager?.Show(
                new Toast("Couldn't save changes."),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);

        SettingsForm.Working = false;
    }

    public async Task DeleteAccount()
    {
        if (!_clickedDelete)
        {
            _clickedDelete = true;
            ToastManager?.Show(
                new Toast("Are you sure? If yes, press \"Delete Account\" again."),
                showIcon: true,
                showClose: false,
                type: NotificationType.Warning,
                classes: ["Light"]);

            return;
        }

        SettingsForm.Working = true;
        var request = await _httpClient.DeleteAsync("/api/Accounts");

        if (request.StatusCode != HttpStatusCode.OK)
        {
            _clickedDelete = false; // In case user changed their mind
            ToastManager?.Show(
                new Toast("Couldn't delete account."),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);

            return;
        }

        await Preferences.RemoveAsync("token");
        _httpClient.DefaultRequestHeaders.Authorization = null;

        if (HostScreen is MainViewModel viewModel)
        {
            viewModel.Authenticated = false;
            viewModel.IsAdmin = false;
        }

        HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
    }
}