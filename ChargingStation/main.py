import RPi.GPIO as GPIO
import charging_station_ports
import server_connection
import display
import tkinter as tk
import time
import charging_status

import configparser

# Load the INI file
config = configparser.ConfigParser()
config.read('config.ini')

# Access values
id = config['Identification']['ID']
password = config["Identification"]["Password"]

server_connection.init(int(id), password)

GPIO.setmode(GPIO.BCM)

charging_station_ports.turn_port_off(0)
charging_station_ports.turn_port_off(1)

root = tk.Tk()
root.title("Stan przycisku")

portName = tk.Label(root, text="Port id: "+str(id), font=("Arial", 24))
portName.pack(pady=20)

statusPort1 = tk.Label(root, text="Port 1: Available", font=("Arial", 24))
statusPort1.pack(pady=20)

statusPort2 = tk.Label(root, text="Port 2: Available", font=("Arial", 24))
statusPort2.pack(pady=20)

def change_port_status(label, port_info):
    if port_info["serviceMode"]:
        label.config(text="PORT 1 IN SERVICE MODE.", fg="red")
    elif port_info["status"] == 0:
        label.config(text="Port 1: Available", fg="green")
    elif port_info["status"] == 1:
        label.config(text="Port 1: Occupied", fg="yellow")

def loop():
    server_connection.get_port(0)
    server_connection.get_port(1)

    ports = server_connection.ports

    change_port_status(statusPort1, ports[0])
    change_port_status(statusPort2, ports[1])
    print(server_connection.ports)

    wattages = charging_status.get_info()
    print(wattages)
    server_connection.set_wattage(0, wattages[0])
    # server_connection.set_wattage(1, wattages[1])

    root.after(1000, loop)

# Start
loop()
root.mainloop()