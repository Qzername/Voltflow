using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models;

public class TwoFactorForm
{
	[Reactive] public string? Code { get; set; }
}

