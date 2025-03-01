namespace VoltflowAPI.Models;

public struct ChargingInfo
{
    public DateTime StartDate { get; set; }
    public int PortId { get; set; }
    public int StationId { get; set; }
    public int CarId { get; set; }
    public bool IsDiscount { get; set; }
}
