using System.Collections.Concurrent;

namespace VoltflowAPI.Services;

public static class ChargeManager
{
    public static readonly ConcurrentDictionary<int, float> Wattages = new();
    public static readonly ConcurrentDictionary<int, DateTime> StartDates = new();
}

