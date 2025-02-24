using Avalonia.Controls.Notifications;
using Avalonia.SimplePreferences;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Ursa.Controls;
using Voltflow.Models;
using Voltflow.Models.Forms;

namespace Voltflow.ViewModels.Account;

/// <summary>
/// ViewModel for AccountView.
/// </summary>
/// <param name="screen"></param>
public class AccountViewModel(IScreen screen) : ViewModelBase(screen)
{
	public WindowToastManager? ToastManager;
	[Reactive] public AuthType CurrentAuthType { get; set; }

	// Forms
	public SignInForm SignInForm { get; set; } = new();
	public SignUpForm SignUpForm { get; set; } = new();
	public TwoFactorAuthForm TwoFactorAuthForm { get; set; } = new();
	public PasswordResetForm PasswordResetForm { get; set; } = new();
	public EmailVerificationForm EmailVerificationForm { get; set; } = new();

	// Commands
	public void NavigateToSettings() => HostScreen.Router.Navigate.Execute(new SettingsViewModel(HostScreen));
	public void NavigateToDiscounts() => HostScreen.Router.Navigate.Execute(new DiscountsViewModel(HostScreen));
	public void NavigateBack() => CurrentAuthType = AuthType.SignIn;
	public void SwitchSignForms() => CurrentAuthType = CurrentAuthType == AuthType.SignIn ? AuthType.SignUp : AuthType.SignIn;
	public void SwitchToPasswordReset() => CurrentAuthType = AuthType.PasswordReset;

	/// <summary>
	/// Sends a request which verifies the email.
	/// </summary>
	public async Task VerifyEmail()
	{
		EmailVerificationForm.Working = true;
		HttpClient client = GetService<HttpClient>();

		var json = new
		{
			EmailVerificationForm.Email,
			TokenModel = new { EmailVerificationForm.Token }
		};
		StringContent content = JsonConverter.ToStringContent(json);
		HttpResponseMessage request = await client.PostAsync("/api/Identity/Authentication/confirm-email", content);

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

	/// <summary>
	/// Sends a request which sends a token to user's email.
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
		HttpClient client = GetService<HttpClient>();

		var json = new { PasswordResetForm.Email };
		StringContent content = JsonConverter.ToStringContent(json);
		HttpResponseMessage request = await client.PostAsync("/api/Identity/PasswordReset/send", content);

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
				new Toast("Something went wrong..."),
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
		HttpClient client = GetService<HttpClient>();

		object json = new
		{
			PasswordResetForm.Email,
			PasswordResetForm.Password,
			TokenModel = new { PasswordResetForm.Token }
		};
		StringContent content = JsonConverter.ToStringContent(json);
		HttpResponseMessage request = await client.PostAsync("/api/Identity/PasswordReset/reset", content);

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
		HttpClient client = GetService<HttpClient>();

		object json = new
		{
			SignInForm.Email,
			SignInForm.Password,
		};
		StringContent content = JsonConverter.ToStringContent(json);
		HttpResponseMessage request = await client.PostAsync("/api/Identity/Authentication/login", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			string stringResponse = await request.Content.ReadAsStringAsync();
			JObject response = JsonConverter.FromString(stringResponse);

			// Request will return 'requires2FA' and 'twoFactorToken' when 2FA is enabled on the account.
			// Prepare stuff and switch to TwoFactorAuthView.
			if (response.ContainsKey("requires2FA") && response.ContainsKey("twoFactorToken"))
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?)response["twoFactorToken"]);
				HttpResponseMessage twoFactorRequest = await client.PostAsync("/api/Identity/TwoFactor/send", null);

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
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?)response["token"]);
				CurrentAuthType = AuthType.SignedIn;
				if (HostScreen is MainViewModel screen)
					screen.Authenticated = true;
				SignInForm.Reset();
			}
		}
		else if (request.StatusCode == HttpStatusCode.Unauthorized)
			ToastManager?.Show(
				new Toast("Provided credentials are invalid or user doesn't exist!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		SignInForm.Working = false;
	}

	/// <summary>
	/// Sends a request which signs up the user.
	/// </summary>
	public async Task SignUp()
	{
		bool emailValid = EmailValidator.IsValid(SignUpForm.Email);
		bool passwordValid = PasswordValidator.IsValid(SignUpForm.Password);
		bool firstNameValid = NameValidator.IsValid(SignUpForm.FirstName);
		bool lastNameValid = NameValidator.IsValid(SignUpForm.LastName);
		bool phoneNumberValid = NumberValidator.IsValid(SignUpForm.PhoneNumber, 9);

		if (!emailValid || !passwordValid || !firstNameValid || !lastNameValid || !phoneNumberValid)
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
		HttpClient client = GetService<HttpClient>();

		object json = new
		{
			SignUpForm.Email,
			Name = SignUpForm.FirstName,
			Surname = SignUpForm.LastName,
			SignUpForm.Password,
			Phone = SignUpForm.PhoneNumber
		};
		StringContent content = JsonConverter.ToStringContent(json);
		HttpResponseMessage request = await client.PostAsync("/api/Identity/Authentication/register", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			EmailVerificationForm.Email = SignUpForm.Email;
			CurrentAuthType = AuthType.EmailVerification;
			SignUpForm.Reset();

			ToastManager?.Show(
				new Toast("Provided email must be verified. A verification token has been sent. Please, paste it below."),
				showIcon: true,
				showClose: false,
				type: NotificationType.Information,
				classes: ["Light"]);
		}
		else if (request.StatusCode == HttpStatusCode.BadRequest)
			ToastManager?.Show(
				new Toast("User already exists!"),
				showIcon: true,
				showClose: false,
				type: NotificationType.Error,
				classes: ["Light"]);

		SignUpForm.Working = false;
	}

	public async Task SignOut()
	{
		await Preferences.SetAsync<string?>("token", null);
		if (HostScreen is MainViewModel screen)
			screen.Authenticated = false;
		CurrentAuthType = AuthType.SignIn;
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
		};

		TwoFactorAuthForm.Working = true;
		HttpClient client = GetService<HttpClient>();

		object json = new
		{
			TwoFactorAuthForm.Token,
		};
		StringContent content = JsonConverter.ToStringContent(json);
		HttpResponseMessage request = await client.PostAsync("/api/Identity/TwoFactor/verify", content);

		if (request.StatusCode == HttpStatusCode.OK)
		{
			string stringResponse = await request.Content.ReadAsStringAsync();
			JObject response = JsonConverter.FromString(stringResponse);

			if (response.ContainsKey("token"))
			{
				await Preferences.SetAsync("token", (string?)response["token"]);
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string?)response["token"]);
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
}

