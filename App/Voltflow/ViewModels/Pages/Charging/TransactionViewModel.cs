using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;

namespace Voltflow.ViewModels.Pages.Charging;

public class TransactionViewModel : ViewModelBase
{
    [Reactive] public bool IsWon { get; set; }
    public bool IsDone
    {
        set
        {
            if (IsWon)
                Text = "You won! -10%";
            else
                Text = "Better luck next time";

            ShowText = true;
        }
    }

    [Reactive] public string Text { get; set; }
    [Reactive] public bool ShowText { get; set; }

    public TransactionViewModel(bool isWon, IScreen screen) : base(screen)
    {
        IsWon = isWon;
    }
}
