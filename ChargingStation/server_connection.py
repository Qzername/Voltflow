import time
from signalrcore.hub_connection_builder import HubConnectionBuilder

station_number = None
def set_station_number(value):
    global station_number
    station_number = value

time_left = None
def set_time_left(value):
    global time_left
    time_left = value

current_power = None
def set_current_power(value):
    global current_power
    current_power = value

port_status = None
def set_port_status(value):
    global port_status
    port_status = value

hub_connection = HubConnectionBuilder()\
    .with_url("http://localhost:5227/testhub", options = { "verify_ssl": False })\
    .with_automatic_reconnect({
        "type": "raw",
        "keep_alive_interval": 10,
        "reconnect_interval": 5,
        "max_attempts": 5
    }).build()

hub_connection.on("PortStatus", lambda args: set_port_status(args[0]))
hub_connection.on("CurrentPower", lambda args: set_current_power(args[0]))
hub_connection.on("TimeLeft", lambda args: set_time_left(args[0]))
hub_connection.on("StationNumber", lambda args: set_station_number(args[0]))

hub_connection.start()

hub_connection.send("GetStationNumber", [])

while True:
    hub_connection.send("GetPortStatus", [])
    hub_connection.send("GetCurrentPower", [])
    hub_connection.send("GetTimeLeft", [])

    time.sleep(1)