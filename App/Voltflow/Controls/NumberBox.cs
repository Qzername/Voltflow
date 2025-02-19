using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Text.RegularExpressions;

namespace Voltflow.Controls;

public partial class NumberBox : TextBox
{
	public NumberBox() { PastingFromClipboard += HandlePasting; }

	public static readonly StyledProperty<bool> AllowFloatsProperty =
		AvaloniaProperty.Register<NumberBox, bool>(nameof(AllowFloats), defaultValue: false);

	public bool AllowFloats
	{
		get => GetValue(AllowFloatsProperty);
		set => SetValue(AllowFloatsProperty, value);
	}

	protected override Type StyleKeyOverride => typeof(TextBox);

	protected override void OnTextInput(TextInputEventArgs e)
	{
		// Only allow 0 to 9 and a dot.
		Regex numberRegex = new Regex($"^[0-9{(AllowFloats ? "." : null)}]$");
		if (string.IsNullOrEmpty(e.Text) || !numberRegex.IsMatch(e.Text)) return;

		if (e.Text == ".")
		{
			if (!AllowFloats) return;
			if (!string.IsNullOrEmpty(Text) && Text.Contains(".")) return; // If Text already contains a dot - return.
			if (Text.Length + 2 > MaxLength && MaxLength != 0) return; // If length of Text + ".0" is higher than maximum length set in the input - return.
			e.Text = ".0";
		}

		base.OnTextInput(e);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		// If "BACK" or "DELETE" key is pressed and there's only a single digit in the input or whole input is selected - set input to "0".
		if ((e.Key == Key.Back || e.Key == Key.Delete) && (Text.Length <= 1 || (SelectionStart == 0 && SelectionEnd == Text.Length)))
		{
			Text = "0";
			return;
		}

		base.OnKeyDown(e);
	}

	private async void HandlePasting(object? sender, RoutedEventArgs e)
	{
		TextBox? textBox = (TextBox?)sender;
		if (textBox == null) return;

		var clipboard = TopLevel.GetTopLevel(textBox)?.Clipboard;
		if (clipboard == null) return;

		string? clipboardText = await clipboard.GetTextAsync();
		if (string.IsNullOrEmpty(clipboardText)) return;

		// Allow pasting numbers and floats (obviously only when AllowFloats is set to true).
		// 3.14, 123.456, 2005 and the list goes on.
		Regex numberRegex = new Regex($"^[0-9]+{(AllowFloats ? "(\\.[0-9]+)?" : "")}$");
		if (!numberRegex.IsMatch(clipboardText)) e.Handled = true;
	}
}