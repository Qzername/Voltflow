using Avalonia.Controls.Notifications;
using ReactiveUI;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Models.Forms;

namespace Voltflow.ViewModels.Account;

/// <summary>
/// ViewModel for AccountView.
/// </summary>
/// <param name="screen"></param>
public class ChangePasswordViewModel : ViewModelBase
{
    public ChangePasswordViewModel(IScreen screen, string email) : base(screen)
    {
        _httpClient = GetService<HttpClient>();
        PasswordForm.Email = email;
    }

    private readonly HttpClient _httpClient;
    public WindowToastManager? ToastManager;

    public PasswordForm PasswordForm { get; set; } = new();

    /// <summary>
    /// Sends a request which resets the password to a new one.
    /// </summary>
    public async Task ResetPassword()
    {
        bool passwordValid = PasswordValidator.IsValid(PasswordForm.Password);
        if (!passwordValid)
        {
            ToastManager?.Show(
                new Toast("Provided credentials do not meet our rules!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);

            return;
        };

        PasswordForm.Working = true;

        var json = new
        {
            PasswordForm.Email,
            PasswordForm.Password,
            TokenModel = new { PasswordForm.Token }
        };
        var content = JsonConverter.ToStringContent(json);
        var request = await _httpClient.PostAsync("/api/Identity/PasswordReset/reset", content);

        if (request.StatusCode == HttpStatusCode.OK)
        {
            PasswordForm.Reset();
            HostScreen.Router.NavigateBack.Execute();
        }
        else
            ToastManager?.Show(
                new Toast("Invalid token!"),
                showIcon: true,
                showClose: false,
                type: NotificationType.Error,
                classes: ["Light"]);

        PasswordForm.Working = false;
    }
}

