using Avalonia.ReactiveUI;
using Voltflow.ViewModels.Account;

namespace Voltflow.Views.Account;

public partial class SettingsView : ReactiveUserControl<SettingsViewModel>
{
    public SettingsView()
    {
        InitializeComponent();
    }
}