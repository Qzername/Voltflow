using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.VisualElements;
using ReactiveUI;
using System.Collections.Generic;
using Voltflow.ViewModels.Pages.Map;

namespace Voltflow.ViewModels.Pages.Statistics;

public class StatisticsViewModel(IScreen screen) : ViewModelBase(screen)
{
	public IEnumerable<ISeries> Series { get; set; } =
		new[] { 2, 4, 1, 4, 3 }.AsPieSeries();

	// the expression above is equivalent to the next series collection:
	public IEnumerable<ISeries> Series2 { get; set; } =
		[
			new PieSeries<int> { Values = [2] },
			new PieSeries<int> { Values = [4] },
			new PieSeries<int> { Values = [1] },
			new PieSeries<int> { Values = [4] },
			new PieSeries<int> { Values = [3] },
		];

	public LabelVisual Title { get; set; } =
		new LabelVisual
		{
			Text = "My chart title",
			TextSize = 25,
			Padding = new LiveChartsCore.Drawing.Padding(15)
		};

	public void NavigateHome() => HostScreen.Router.NavigateAndReset.Execute(new MapViewModel(HostScreen));
}