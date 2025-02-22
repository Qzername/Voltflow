using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Account;

namespace Voltflow.Views.Account;

public partial class DiscountsView : ReactiveUserControl<DiscountsViewModel>
{
    public DiscountsView()
    {
        InitializeComponent();
    }
}