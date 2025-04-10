from signalrcore.hub_connection_builder import HubConnectionBuilder
from charging_station_ports import turn_port_on, turn_port_off

def init():
    client = HubConnectionBuilder()\
        .with_url("https://voltflow-api.heapy.xyz/picharginghub")\
        .with_automatic_reconnect({
            "type": "raw",
            "keep_alive_interval": 10,
            "reconnect_interval": 5,
            "max_attempts": 5
        }).build()

    client.on("TurnPortOn", lambda args: turn_port_on(args[0]))
    client.on("TurnPortOff", lambda args: turn_port_off(args[0]))

    try:
        client.start()
    except:
        print("Couldn't connect to server")