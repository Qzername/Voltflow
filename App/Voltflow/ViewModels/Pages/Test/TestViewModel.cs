using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;

namespace Voltflow.ViewModels.Pages.Test;

public class TestViewModel : ViewModelBase
{
    [Reactive] public bool IsWon { get; set; } = true;
    public bool IsDone
    {
        set
        {
            Debug.WriteLine("Card completed");
        }
    }

    public TestViewModel(IScreen screen) : base(screen)
    {
        IsWon = true;
    }
}
