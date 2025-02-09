using System.ComponentModel.DataAnnotations;

public class Car
{
    public string Id { get; set; }
    public string AccountId { get; set; }
    [Range(0,100)]
    public string Name { get; set; }
    public int ChargingRate { get; set; }
}