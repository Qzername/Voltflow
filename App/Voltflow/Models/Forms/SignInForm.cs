using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Represents the sign-in form.
/// </summary>
public class SignInForm : BaseForm
{
    [Reactive] public string? Email { get; set; }
    [Reactive] public string? Password { get; set; }
    [Reactive] public bool ShowPassword { get; set; }

    public void TogglePassword() => ShowPassword = !ShowPassword;

    /// <summary>
    /// Resets variables stored in the form.
    /// </summary>
    public virtual void Reset()
    {
        Email = null;
        Password = null;
        ShowPassword = false;
        Working = false;
    }
}

