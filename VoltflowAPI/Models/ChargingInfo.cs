namespace VoltflowAPI.Models;

public struct ChargingInfo
{
    public DateTime StartDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public int PortId { get; set; }
    public int StationId { get; set; }
    public int CarId { get; set; }
    public double EnergyConsumed { get; set; }
    public double TotalCost { get; set; }
    public bool IsDiscount { get; set; }
    public bool Started { get; set; }
    public bool Disconnected { get; set; }
}
