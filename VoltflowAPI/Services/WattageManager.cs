using System.Collections.Concurrent;

namespace VoltflowAPI.Services;

public static class WattageManager
{
    public static readonly ConcurrentDictionary<int, float> Wattages = new();
}

