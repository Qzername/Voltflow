using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voltflow.Models;
public struct ChargingPort
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string Name { get; set; }
    public ChargingPortStatus Status { get; set; }
    public bool ServiceMode { get; set; }
}

public enum ChargingPortStatus
{
    Available,
    Occupied,
    OutOfService,
}