namespace VoltflowAPI.Models.Application;

public class ChargingStation
{
    public int Id { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int Cost { get; set; }
    public int MaxChargeRate { get; set; }
    public string Password { get; set; }
    public string? Message { get; set; }
    public ICollection<ChargingPort> Ports { get; set; }
    public ICollection<ChargingStationOpeningHours> OpeningHours { get; set; }
}