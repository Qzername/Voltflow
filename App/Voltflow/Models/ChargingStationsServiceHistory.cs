using System;

namespace Voltflow.Models;

public struct ChargingStationsServiceHistory
{
	public int Id { get; set; }
	public int StationId { get; set; }
	public DateTime EndDate { get; set; }
}
