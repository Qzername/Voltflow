namespace VoltflowAPI.Models.Application;

public class ChargingStation
{
    public int Id { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int Cost { get; set; }
    public int MaxChargeRate { get; set; }
    public int Status { get; set; }
    public bool ServiceMode { get; set; }
}

public enum ChargingStationStatus
{
    Available,
    Occupied,
    OutOfOrder,
}