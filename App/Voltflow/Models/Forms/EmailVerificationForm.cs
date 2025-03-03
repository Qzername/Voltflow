using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Represents the password reset form.
/// </summary>
public class EmailVerificationForm : BaseForm
{
    [Reactive] public string? Email { get; set; }
    [Reactive] public string? Token { get; set; }

    /// <summary>
    /// Resets variables stored in the form.
    /// </summary>
    public void Reset()
    {
        Email = null;
        Token = null;
        Working = false;
    }
}
