using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Pages.Test;

namespace Voltflow.Views.Pages.Test;

public partial class TestView : ReactiveUserControl<TestViewModel>
{
    public TestView()
    {
        InitializeComponent();
    }
}