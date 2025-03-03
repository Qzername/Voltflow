using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Represents the 2FA form.
/// </summary>
public class TwoFactorAuthForm : BaseForm
{
    [Reactive] public string? Token { get; set; }

    /// <summary>
    /// Resets variables stored in the form.
    /// </summary>
    public void Reset()
    {
        Token = null;
        Working = false;
    }
}

