from signalrcore.hub_connection_builder import HubConnectionBuilder
import charging_station_ports

ports = {
    0: {
        "status":0,
        "serviceMode":False
    },
    1: {
        "status":0,
        "serviceMode":False
    }
}

message = None

connected = False
def on_connected():
    global connected
    connected = True

def on_disconnected():
    global connected
    connected = False

def manage_port(port):
    ports[port["index"]]["status"] = port["status"]
    ports[port["index"]]["serviceMode"] = port["serviceMode"]

    if (port["status"] == 1 and port["serviceMode"] == False):
        charging_station_ports.turn_port_on(port["index"])
    else:
        charging_station_ports.turn_port_off(port["index"])

def manage_message(new_message):
    global message
    message = new_message
    print(new_message)

def init(id, password):
    global client

    client = HubConnectionBuilder()\
        .with_url(f"https://voltflow-api.heapy.xyz/picharginghub?stationId={id}&password={password}")\
        .with_automatic_reconnect({
            "type": "raw",
            "keep_alive_interval": 60,
            "reconnect_interval": 2,
            "max_attempts": 10
        }).build()
    
    client.on_close(lambda: on_disconnected())
    client.on_open(lambda: on_connected())
    
    client.on("Error", lambda args: print("Error:", args[0]))
    client.on("Port", lambda args: manage_port(args[0]))
    client.on("Message", lambda args: manage_message(args[0]))

    try:
        client.start()
    except Exception as e:
        print("Couldn't connect to server:", e)

def get_port(index):
    global client

    client.send("GetPort", [index])

def get_message():
    global client

    client.send("GetMessage", [])

def set_wattage(index, wattage):
    global client

    client.send("SetWattage", [index, wattage])
