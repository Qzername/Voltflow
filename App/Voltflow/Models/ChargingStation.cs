namespace Voltflow.Models;

public struct ChargingStation
{
    public int Id { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int Cost { get; set; }
    public int MaxChargeRate { get; set; }
    public ChargingPort[] Ports { get; set; }
    public ChargingStationOpeningHours OpeningHours { get; set; }
}