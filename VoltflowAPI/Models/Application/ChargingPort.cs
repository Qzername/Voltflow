namespace VoltflowAPI.Models.Application;

public class ChargingPort
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public int Status { get; set; }
    public bool ServiceMode { get; set; }
    public string Name { get; set; }

    public ChargingStation Station { get; set; }
}

public enum ChargingPortStatus
{
    Available,
    Occupied,
    OutOfService,
}