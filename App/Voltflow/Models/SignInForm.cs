using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models;

/// <summary>
/// Represents the sign-in form.
/// </summary>
public class SignInForm
{
	[Reactive] public string? Email { get; set; }
	[Reactive] public string? Password { get; set; }
}

