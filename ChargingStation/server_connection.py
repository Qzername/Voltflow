from signalrcore.hub_connection_builder import HubConnectionBuilder
import charging_station_ports

def manage_port(port):
    if (port["status"] == 0 and port["serviceMode"] == False):
        charging_station_ports.turn_port_on(port["id"])
    else:
        charging_station_ports.turn_port_off(port["id"])

def init(id, password):
    global client

    client = HubConnectionBuilder()\
        .with_url(f"https://voltflow-api.heapy.xyz/picharginghub?stationId={id}&password={password}")\
        .with_automatic_reconnect({
            "type": "raw",
            "keep_alive_interval": 10,
            "reconnect_interval": 5,
            "max_attempts": 5
        }).build()
    
    client.on("Error", lambda args: print("Error:", args[0]))
    client.on("Port", lambda args: manage_port(args[0]))

    try:
        client.start()
    except Exception as e:
        print("Couldn't connect to server:", e)

def get_port(index):
    global client

    client.send("GetPort", [index])