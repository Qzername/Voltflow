using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Voltflow.Models.Forms;

/// <summary>
/// Base for other forms.
/// </summary>
public class BaseForm : ReactiveObject
{
	[Reactive] public bool Working { get; set; }
}

