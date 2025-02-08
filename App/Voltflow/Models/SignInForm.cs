using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models;

public class SignInForm
{
	[Reactive] public string? Email { get; set; }
	[Reactive] public string? Password { get; set; }
}

