using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace VoltflowAPI.Hubs;

public class ChargingHub : Hub
{
    private static readonly ConcurrentDictionary<string, DateTime> _connections = new();

    public override Task OnConnectedAsync()
    {
        _connections[Context.ConnectionId] = DateTime.UtcNow;
        Console.WriteLine("new conn");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var startTime = _connections[Context.ConnectionId];
        var timePassed = DateTime.UtcNow - startTime;

        Console.WriteLine(timePassed);

        _connections.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}
