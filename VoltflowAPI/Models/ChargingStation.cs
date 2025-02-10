namespace VoltflowAPI.Models;

public class ChargingStation
{
    public int Id { get; set; }
    public string Status { get; set; }
    public double Longitude { get; set; }
    public double Latitude { get; set; }
    public int Cost { get; set; }
    public int MaxChargeRate { get; set; }  
}
