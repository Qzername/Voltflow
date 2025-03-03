namespace Voltflow.Models;

public class ChargingPort
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string? Name { get; set; }
    public ChargingPortStatus Status { get; set; }
    public bool ServiceMode { get; set; }
}

public enum ChargingPortStatus
{
    Available,
    Occupied,
    OutOfService,
}