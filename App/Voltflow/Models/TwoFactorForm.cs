using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models;

/// <summary>
/// Represents the 2FA form.
/// </summary>
public class TwoFactorForm
{
	[Reactive] public string? Code { get; set; }
}

