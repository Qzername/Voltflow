using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using Voltflow.Models;

namespace Voltflow.ViewModels;

public class AuthViewModel(IScreen screen) : ViewModelBase(screen)
{
	[Reactive] public AuthType AuthType { get; set; } = AuthType.SignIn;
	[Reactive] public bool Authenticating { get; set; }
	[Reactive] public bool ShowPassword { get; set; }

	public SignInForm SignInForm { get; set; } = new();
	public SignUpForm SignUpForm { get; set; } = new();
	public TwoFactorForm TwoFactorForm { get; set; } = new();

	public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new TestViewModel(screen));
	public void NavigateBack() => AuthType = AuthType.SignIn;
	public void SwitchForms() => AuthType = AuthType == AuthType.SignIn ? AuthType.SignUp : AuthType.SignIn;
	public void RevealPassword() => ShowPassword = !ShowPassword;

	public void ValidateSignIn()
	{
		bool emailValid = EmailValidator.IsValid(SignInForm.Email);
		bool passwordValid = PasswordValidator.IsValid(SignInForm.Password);

		Debug.WriteLine($"Email is {(emailValid ? "VALID" : "INVALID")}!");
		Debug.WriteLine($"Password is {(passwordValid ? "VALID" : "INVALID")}!");
	}

	public void ValidateSignUp()
	{
		bool emailValid = EmailValidator.IsValid(SignUpForm.Email);
		bool passwordValid = PasswordValidator.IsValid(SignUpForm.Password);
		bool firstNameValid = NameValidator.IsValid(SignUpForm.FirstName);
		bool lastNameValid = NameValidator.IsValid(SignUpForm.LastName);
		bool phoneNumberValid = NumberValidator.IsValid(SignUpForm.PhoneNumber, 9);

		Debug.WriteLine($"Email is {(emailValid ? "VALID" : "INVALID")}!");
		Debug.WriteLine($"Password is {(passwordValid ? "VALID" : "INVALID")}!");
		Debug.WriteLine($"First name is {(firstNameValid ? "VALID" : "INVALID")}!");
		Debug.WriteLine($"Last name is {(lastNameValid ? "VALID" : "INVALID")}!");
		Debug.WriteLine($"Phone number is {(phoneNumberValid ? "VALID" : "INVALID")}!");
	}

	public void ValidateTwoFactor()
	{
		bool codeValid = NumberValidator.IsValid(TwoFactorForm.Code, 6);
		Debug.WriteLine($"2FA code is {(codeValid ? "VALID" : "INVALID")}!");
	}
}

