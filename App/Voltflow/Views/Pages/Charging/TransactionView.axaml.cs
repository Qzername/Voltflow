using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Charging;

namespace Voltflow.Views.Pages.Charging;

public partial class TransactionView : ReactiveUserControl<TransactionViewModel>
{
    public TransactionView()
    {
        InitializeComponent();
    }
}