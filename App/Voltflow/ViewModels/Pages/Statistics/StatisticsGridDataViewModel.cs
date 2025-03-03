using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Reactive;
using Voltflow.Models;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsGridDataViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, IRoutableViewModel> GoBack => HostScreen.Router.NavigateBack;

    [Reactive] public List<GridElement>? Elements { get; set; }
    [Reactive] public List<StationGridElement>? Stations { get; set; }
    [Reactive] public List<TransactionGridElement>? Transactions { get; set; }
    [Reactive] public StatisticsType CurrentStatisticsType { get; set; }

    public StatisticsGridDataViewModel(
        IScreen screen,
        List<GridElement>? elements = null,
        List<StationGridElement>? stations = null,
        List<TransactionGridElement>? transactions = null
    ) : base(screen)
    {
        Elements = elements ?? [];
        Stations = stations ?? [];
        Transactions = transactions ?? [];

        if (Elements.Count > 0)
            CurrentStatisticsType = StatisticsType.Default;
        else if (Stations.Count > 0)
            CurrentStatisticsType = StatisticsType.Stations;
        else if (Transactions.Count > 0)
            CurrentStatisticsType = StatisticsType.Transactions;
    }
}