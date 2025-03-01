namespace VoltflowAPI.Models.Application;

public class ChargingStationOpeningHours
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

    public TimeSpan[] GetToday() => DateTime.UtcNow.DayOfWeek switch
    {
        DayOfWeek.Monday => Monday,
        DayOfWeek.Tuesday => Tuesday,
        DayOfWeek.Wednesday => Wednesday,
        DayOfWeek.Thursday => Thursday,
        DayOfWeek.Friday => Friday,
        DayOfWeek.Saturday => Saturday,
        DayOfWeek.Sunday => Sunday,
        _ => throw new Exception("Invalid day of the week")
    };
}
