using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.SimplePreferences;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Models.Forms;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Account;

/// <summary>
/// ViewModel for AccountView.
/// </summary>
/// <param name="screen"></param>
public class AccountViewModel : ViewModelBase
{
	public AccountViewModel(IScreen screen) : base(screen)
	{
		_httpClient = GetService<HttpClient>();
	}

	private readonly HttpClient _httpClient;
	public WindowToastManager? ToastManager;
	[Reactive] public AuthType CurrentAuthType { get; set; }

	#region Forms
	public SignInForm SignInForm { get; set; } = new();
	public SignUpForm SignUpForm { get; set; } = new();
	public TwoFactorAuthForm TwoFactorAuthForm { get; set; } = new();
	public PasswordResetForm PasswordResetForm { get; set; } = new();
	public EmailVerificationForm EmailVerificationForm { get; set; } = new();
	#endregion

	#region Commands
	public void NavigateBack() => CurrentAuthType = AuthType.SignIn;
	public void SwitchSignForms() => CurrentAuthType = CurrentAuthType == AuthType.SignIn ? AuthType.SignUp : AuthType.SignIn;
	public void SwitchToPasswordReset() => CurrentAuthType = AuthType.PasswordReset;

	// Prefetch account's name, surname, phone number and 2FA status.
	public async Task NavigateToSettings()
	{
		SettingsForm settingsForm = new();
		var request = await _httpClient.GetAsync("/api/Accounts");
		var twoFactorRequest = await _httpClient.GetAsync("/api/Identity/TwoFactor/status");

		if (request.StatusCode != HttpStatusCode.OK || twoFactorRequest.StatusCode != HttpStatusCode.OK)
		{
			ToastManager?.Show(
				new Toast("Couldn't prefetch account's information!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

			return;
		}

		var stringResponse = await request.Content.ReadAsStringAsync();
		var response = JsonConverter.FromString(stringResponse);

		var stringTwoFactorResponse = await twoFactorRequest.Content.ReadAsStringAsync();
		var twoFactorResponse = JsonConverter.FromString(stringTwoFactorResponse);

		if (response.ContainsKey("name"))
			settingsForm.Name = (string?)response["name"];
		if (response.ContainsKey("surname"))
			settingsForm.Surname = (string?)response["surname"];
		if (response.ContainsKey("phoneNumber"))
			settingsForm.PhoneNumber = (string?)response["phoneNumber"];
		if (twoFactorResponse.ContainsKey("twoFactorEnabled"))
			settingsForm.TwoFactor = (bool)twoFactorResponse["twoFactorEnabled"]!;

		HostScreen.Router.Navigate.Execute(new SettingsViewModel(HostScreen, settingsForm, Application.Current?.RequestedThemeVariant));
	}
	#endregion

	// Signs out user, removes saved token.
	public async Task SignOut()
	{
		await Preferences.RemoveAsync("token");
		_httpClient.DefaultRequestHeaders.Authorization = null;

		if (HostScreen is MainViewModel screen)
			screen.Authenticated = false;

		CurrentAuthType = AuthType.SignIn;
	}

	#region Signing in
	/// <summary>
	/// Sends a request which signs in the user.
	/// </summary>
	public async Task SignIn()
	{
		bool emailValid = EmailValidator.IsValid(SignInForm.Email);
		bool passwordValid = PasswordValidator.IsValid(SignInForm.Password);

		if (!emailValid || !passwordValid)
		{
			ToastManager?.Show(
				new Toast("Provided credentials do not meet our rules!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

			return;
		}

		SignInForm.Working = true;

		var json = new
		{
			SignInForm.Email,
			SignInForm.Password,
		};
		var content = JsonConverter.ToStringContent(json);
		var request = await _httpClient.PostAsync("/api/Identity/Authentication/login", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			var stringResponse = await request.Content.ReadAsStringAsync();
			var response = JsonConverter.FromString(stringResponse);

			// Request will return 'requires2FA' and 'twoFactorToken' when 2FA is enabled on the account.
			// Prepare stuff and switch to TwoFactorAuthView.
			if (response.ContainsKey("requires2FA") && response.ContainsKey("twoFactorToken"))
			{
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?)response["twoFactorToken"]);
				var twoFactorRequest = await _httpClient.PostAsync("/api/Identity/TwoFactor/send", null);

				if (twoFactorRequest.StatusCode == HttpStatusCode.OK)
				{
					CurrentAuthType = AuthType.TwoFactorAuth;

					ToastManager?.Show(
						new Toast("A Two-Factor Authentication (2FA) code has been sent to your email. Please, provide it below."),
						showIcon: true,
						showClose: false,
						type: NotificationType.Information,
						classes: ["Light"]);
				}
			}
			// Request will return 'token' when 2FA is disabled on the account.
			// Set 'Authorization' header to 'token' and return to home.
			else if (response.ContainsKey("token"))
			{
				await Preferences.SetAsync("token", (string?)response["token"]);
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?)response["token"]);

				if (HostScreen is MainViewModel screen)
					screen.Authenticated = true;

				SignInForm.Reset();

				HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
			}
		}
		else if (request.StatusCode == HttpStatusCode.Unauthorized)
			ToastManager?.Show(
				new Toast("Provided credentials are invalid or account doesn't exist!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		SignInForm.Working = false;
	}

	/// <summary>
	/// Sends a request which verifies 2FA.
	/// </summary>
	public async Task VerifyTwoFactorAuth()
	{
		bool codeValid = NumberValidator.IsValid(TwoFactorAuthForm.Token, 6);
		if (!codeValid)
		{
			ToastManager?.Show(
				new Toast("Provided credentials do not meet our rules!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

			return;
		}

		TwoFactorAuthForm.Working = true;

		var json = new
		{
			TwoFactorAuthForm.Token,
		};
		var content = JsonConverter.ToStringContent(json);
		var request = await _httpClient.PostAsync("/api/Identity/TwoFactor/verify", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			var stringResponse = await request.Content.ReadAsStringAsync();
			var response = JsonConverter.FromString(stringResponse);

			if (response.ContainsKey("token"))
			{
				await Preferences.SetAsync("token", (string?)response["token"]);
				_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?)response["token"]);

				CurrentAuthType = AuthType.SignedIn;

				if (HostScreen is MainViewModel screen)
					screen.Authenticated = true;

				TwoFactorAuthForm.Reset();
			}
		}
		else
			ToastManager?.Show(
				new Toast("Invalid 2FA code!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		TwoFactorAuthForm.Working = false;
	}
	#endregion

	#region Password reset
	/// <summary>
	/// Sends a request which sends a token to account's email.
	/// </summary>
	public async Task GetResetPasswordToken()
	{
		bool emailValid = EmailValidator.IsValid(PasswordResetForm.Email);
		if (!emailValid)
		{
			ToastManager?.Show(
				new Toast("Provided credentials do not meet our rules!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

			return;
		};

		PasswordResetForm.Working = true;

		var json = new { PasswordResetForm.Email };
		var content = JsonConverter.ToStringContent(json);
		var request = await _httpClient.PostAsync("/api/Identity/PasswordReset/send", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			PasswordResetForm.SentToken = true;

			ToastManager?.Show(
				new Toast("Password reset token has been sent to your email. Please, paste it below."),
				showIcon: true,
				showClose: false,
				type: NotificationType.Information,
				classes: ["Light"]);
		}
		else
			ToastManager?.Show(
				new Toast("Unknown error!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		PasswordResetForm.Working = false;
	}

	/// <summary>
	/// Sends a request which resets the password to a new one.
	/// </summary>
	public async Task ResetPassword()
	{
		bool passwordValid = PasswordValidator.IsValid(PasswordResetForm.Password);
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

		PasswordResetForm.Working = true;

		var json = new
		{
			PasswordResetForm.Email,
			PasswordResetForm.Password,
			TokenModel = new { PasswordResetForm.Token }
		};
		var content = JsonConverter.ToStringContent(json);
		var request = await _httpClient.PostAsync("/api/Identity/PasswordReset/reset", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			CurrentAuthType = AuthType.SignIn;
			PasswordResetForm.Reset();
		}
		else
			ToastManager?.Show(
				new Toast("Invalid token!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		PasswordResetForm.Working = false;
	}
	#endregion

	#region Signing up
	/// <summary>
	/// Sends a request which signs up the user.
	/// </summary>
	public async Task SignUp()
	{
		bool emailValid = EmailValidator.IsValid(SignUpForm.Email);
		bool passwordValid = PasswordValidator.IsValid(SignUpForm.Password);
		bool nameValid = NameValidator.IsValid(SignUpForm.Name);
		bool surnameValid = NameValidator.IsValid(SignUpForm.Surname);
		bool phoneNumberValid = NumberValidator.IsValid(SignUpForm.PhoneNumber, 9);

		if (!emailValid || !passwordValid || !nameValid || !surnameValid || !phoneNumberValid)
		{
			ToastManager?.Show(
				new Toast("Provided credentials do not meet our rules!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

			return;
		}

		SignUpForm.Working = true;

		var json = new
		{
			SignUpForm.Email,
			Name = SignUpForm.Name,
			Surname = SignUpForm.Surname,
			SignUpForm.Password,
			Phone = SignUpForm.PhoneNumber
		};
		var content = JsonConverter.ToStringContent(json);
		var request = await _httpClient.PostAsync("/api/Identity/Authentication/register", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			EmailVerificationForm.Email = SignUpForm.Email;

			// Transfer email and password to SignInForm for better UX.
			SignInForm.Email = SignUpForm.Email;
			SignInForm.Password = SignUpForm.Password;

			CurrentAuthType = AuthType.EmailVerification;
			SignUpForm.Reset();

			ToastManager?.Show(
				new Toast(
					"Provided email must be verified. A verification token has been sent. Please, paste it below."),
				showIcon: true,
				showClose: false,
				type: NotificationType.Information,
				classes: ["Light"]);
		}
		else if (request.StatusCode == HttpStatusCode.BadRequest)
			ToastManager?.Show(
				new Toast("Account already exists!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		SignUpForm.Working = false;
	}

	/// <summary>
	/// Sends a request which verifies the email.
	/// </summary>
	public async Task VerifyEmail()
	{
		EmailVerificationForm.Working = true;

		var json = new
		{
			EmailVerificationForm.Email,
			TokenModel = new { EmailVerificationForm.Token }
		};
		var content = JsonConverter.ToStringContent(json);
		var request = await _httpClient.PostAsync("/api/Identity/Authentication/confirm-email", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			EmailVerificationForm.Reset();
			CurrentAuthType = AuthType.SignIn;

			ToastManager?.Show(
				new Toast("Email verified successfully! You can now sign in."),
				showIcon: true,
				showClose: false,
				type: NotificationType.Success,
				classes: ["Light"]);
		}
		else
			ToastManager?.Show(
				new Toast("Invalid token!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		EmailVerificationForm.Working = false;
	}
	#endregion
}

