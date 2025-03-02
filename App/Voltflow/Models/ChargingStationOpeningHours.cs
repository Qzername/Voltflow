using System;

namespace Voltflow.Models;

public struct ChargingStationOpeningHours
{
	public int Id { get; set; }
	public int StationId { get; set; }
	public TimeSpan[] Monday { get; set; }
	public TimeSpan[] Tuesday { get; set; }
	public TimeSpan[] Wednesday { get; set; }
	public TimeSpan[] Thursday { get; set; }
	public TimeSpan[] Friday { get; set; }
	public TimeSpan[] Saturday { get; set; }
	public TimeSpan[] Sunday { get; set; }

	public static TimeSpan[] GetToday(ChargingStationOpeningHours openingHours) => DateTime.Now.DayOfWeek switch
	{
		DayOfWeek.Monday => openingHours.Monday,
		DayOfWeek.Tuesday => openingHours.Tuesday,
		DayOfWeek.Wednesday => openingHours.Wednesday,
		DayOfWeek.Thursday => openingHours.Thursday,
		DayOfWeek.Friday => openingHours.Friday,
		DayOfWeek.Saturday => openingHours.Saturday,
		DayOfWeek.Sunday => openingHours.Sunday,
		_ => throw new Exception("Invalid day of the week")
	};
}