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

statusPort1 = tk.Label(root, text="Port 1: Available", font=("Arial", 24))
statusPort1.pack(pady=20)

statusPort2 = tk.Label(root, text="Port 2: Available", font=("Arial", 24))
statusPort2.pack(pady=20)

def loop():
    server_connection.get_port(0)
    server_connection.get_port(1)

    print(server_connection.ports)

    wattages = charging_status.get_info()
    server_connection.set_wattage(0, wattages[0])
    server_connection.set_wattage(1, wattages[1])

    root.after(1000, loop)

# Start
loop()
root.mainloop()