using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Voltflow.Models;

/// <summary>
/// Enum with types for StatisticsDataViewModel to know which data to display.
/// </summary>
public enum StatisticsType
{
	Default = 0,
	Stations = 1,
	Transactions = 2
}

/// <summary>
/// Checks if CurrentStatisticsType is the same as the one in a DataGrid.
/// Using this is better than having multiple booleans bound to the 'IsVisible' property of each form.
/// Also, it makes the code look cleaner. ;)
/// </summary>
public class StatisticsTypeConverter : IValueConverter
{
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is StatisticsType statisticsType && parameter is string paramString)
			if (Enum.TryParse(paramString, out StatisticsType paramStatisticsType))
				return statisticsType == paramStatisticsType;

		return false;
	}

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
		throw new NotSupportedException();
}

public class GridElement
{
	public string? CarName { get; set; }
	public int? StationId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public double EnergyConsumed { get; set; }
	public double Cost { get; set; }
}

public class TransactionGridElement
{
	public int? StationId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public double EnergyConsumed { get; set; }
	public double Cost { get; set; }
}

public class StationGridElement
{
	public int StationId { get; set; }
	public string Warning { get; set; }
	public double Latitude { get; set; }
	public double Longitude { get; set; }
	public DateTime? LastCharge { get; set; }
	public int NumberOfChargers { get; set; }
}