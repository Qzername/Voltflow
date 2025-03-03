using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Ursa.Controls;
using Voltflow.ViewModels.Pages.Statistics;

namespace Voltflow.Views.Pages.Statistics;

public partial class StatisticsView : ReactiveUserControl<StatisticsViewModel>
{
    public StatisticsView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is StatisticsViewModel viewModel)
        {
            viewModel.Parent = this;

            var topLevel = TopLevel.GetTopLevel(this);
            viewModel.ToastManager = new WindowToastManager(topLevel) { MaxItems = 1 };
        }
    }
}