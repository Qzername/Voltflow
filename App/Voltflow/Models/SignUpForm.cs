using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models;

public class SignUpForm
{
	[Reactive] public string? Email { get; set; }
	[Reactive] public string? Password { get; set; }
	[Reactive] public string? FirstName { get; set; }
	[Reactive] public string? LastName { get; set; }
	[Reactive] public string? PhoneNumber { get; set; }
}
