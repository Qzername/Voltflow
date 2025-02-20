namespace VoltflowAPI.Models;

public struct ChargingInfo
{
    public DateTime StartDate { get; set; }
    public int StationId { get; set; }
    public int CarId { get; set; }
}
