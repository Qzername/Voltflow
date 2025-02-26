using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Represents the settings form.
/// </summary>
public class SettingsForm : BaseForm
{
	[Reactive] public bool TwoFactor { get; set; }
	[Reactive] public string? Name { get; set; }
	[Reactive] public string? Surname { get; set; }
	[Reactive] public string? PhoneNumber { get; set; }

	/// <summary>
	/// Resets variables stored in the form.
	/// </summary>
	public void Reset()
	{
		TwoFactor = false;
		Name = null;
		Surname = null;
		PhoneNumber = null;
		Working = false;
	}
}
