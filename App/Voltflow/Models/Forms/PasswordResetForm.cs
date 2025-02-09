using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Represents the password reset form.
/// </summary>
public class PasswordResetForm : SignInForm
{
	[Reactive] public string? Token { get; set; }
	[Reactive] public bool SentToken { get; set; }

	/// <summary>
	/// Resets variables stored in the form.
	/// </summary>
	public override void Reset()
	{
		Email = null;
		Password = null;
		ShowPassword = false;
		Token = null;
		SentToken = false;
		Working = false;
	}
}
