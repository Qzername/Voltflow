using System.ComponentModel.DataAnnotations;

namespace VoltflowAPI.Models.Application;

public class Car
{
    public int Id { get; set; }
    public string AccountId { get; set; }
    [Range(0, 100)]
    public string Name { get; set; }
    public int BatteryCapacity { get; set; }
    public int ChargingRate { get; set; }
}