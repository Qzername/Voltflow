namespace Voltflow.Models;

public struct ChargingStation
{
    public int Id { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int Cost { get; set; }
    public int MaxChargeRate { get; set; }
    public ChargingPort[] ChargingPorts { get; set; }
    public ChargingStationOpeningHours ChargingStationOpeningHours { get; set; }
}