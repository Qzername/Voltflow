using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Represents the sign-up form.
/// </summary>
public class SignUpForm : SignInForm
{
    [Reactive] public string? Name { get; set; }
    [Reactive] public string? Surname { get; set; }
    [Reactive] public string? PhoneNumber { get; set; }

    /// <summary>
    /// Resets variables stored in the form.
    /// </summary>
    public override void Reset()
    {
        Email = null;
        Password = null;
        ShowPassword = false;
        Name = null;
        Surname = null;
        PhoneNumber = null;
        Working = false;
    }
}
