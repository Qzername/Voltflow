using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Voltflow.ViewModels.Pages.Charging;

public class TransactionViewModel(double cost, double energyUsed, bool isWon, IScreen screen) : ViewModelBase(screen)
{
	[Reactive] public double Cost { get; set; } = cost;
	[Reactive] public double EnergyConsumed { get; set; } = energyUsed;
	[Reactive] public bool IsWon { get; set; } = isWon;

	public bool IsDone
	{
		set
		{
			Text = IsWon ? "You won! -10%" : "Better luck next time...";

			if(IsWon)
                Cost *= 0.9;

            ShowText = true;
		}
	}

	[Reactive] public string? Text { get; set; }
	[Reactive] public bool ShowText { get; set; }

	public void GenerateInvoice()
	{

	}
}