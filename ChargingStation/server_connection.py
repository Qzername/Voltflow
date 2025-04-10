from signalrcore.hub_connection_builder import HubConnectionBuilder
import charging_station_ports

def init(id, password):
    client = HubConnectionBuilder()
        .with_url("https://voltflow-api.heapy.xyz/picharginghub")
        .with_automatic_reconnect({
            "type": "raw",
            "keep_alive_interval": 10,
            "reconnect_interval": 5,
            "max_attempts": 5
        }).build()

    client.on("TurnPortOn", lambda args: charging_station_ports.turn_port_on(args[0]))
    client.on("TurnPortOff", lambda args: charging_station_ports.turn_port_off(args[0]))

    try:
        client.start()
    except Exception as e:
        print("Couldn't connect to server:", e)