using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Voltflow.ViewModels.Pages.Charging;

public class TransactionViewModel(bool isWon, IScreen screen) : ViewModelBase(screen)
{
	[Reactive] public bool IsWon { get; set; } = isWon;

	public bool IsDone
	{
		set
		{
			Text = IsWon ? "You won! -10%" : "Better luck next time...";
			ShowText = true;
		}
	}

	[Reactive] public string? Text { get; set; }
	[Reactive] public bool ShowText { get; set; }
}
