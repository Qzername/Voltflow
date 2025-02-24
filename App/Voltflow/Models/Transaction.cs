using System;

namespace Voltflow.Models;

public struct Transaction
{
	public int Id { get; set; }
	public string AccountId { get; set; }
	public int CarId { get; set; }
	public int ChargingStationId { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public double EnergyConsumed { get; set; }
	public double Cost { get; set; }
}

