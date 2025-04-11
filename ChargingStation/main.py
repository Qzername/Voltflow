import RPi.GPIO as GPIO
import charging_station_ports
import server_connection
import display
import tkinter as tk
import time
import charging_status
import ads
import ads_matrix

from time import sleep
import configparser

# Load the INI file
config = configparser.ConfigParser()
config.read("config.ini")

# Access values
id = config["Identification"]["ID"]
password = config["Identification"]["Password"]

buzzerIsOn = bool(int(config["Buzzer"]["IsOn"]))

server_connection.init(int(id), password)

while not server_connection.connected:
    print("Waiting for connection...")
    time.sleep(1)

server_connection.get_message()
time.sleep(1)

server_connection.get_station()
time.sleep(1)

GPIO.setmode(GPIO.BCM)

buzzer=16
GPIO.setup(buzzer,GPIO.OUT)
GPIO.output(buzzer,GPIO.LOW)

SWITCH_PIN = 26  # Use BCM numbering
GPIO.setup(SWITCH_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)  # Pull-up

charging_station_ports.turn_port_off(0)
charging_station_ports.turn_port_off(1)

root = tk.Tk()
root.title("Voltflow charging station")

# --- Kontener główny: lewo/prawo ---
frame = tk.Frame(root)
frame.pack(fill=tk.BOTH, expand=True)

# --- Lewa część: tekst przycisku itp. ---
left_frame = tk.Frame(frame)
left_frame.pack(side=tk.LEFT, padx=8, pady=8)

portName = tk.Label(root, text="Station id: "+str(id), font=("Arial", 24))
portName.pack(pady=8)

if (server_connection.message != None):
    stationMessage = tk.Label(root, text="Message: "+server_connection.message, font=("Arial", 16))
    stationMessage.pack(pady=8)

stationCost = tk.Label(root, text="Cost: "+str(server_connection.station["cost"])+" zł/kWh", font=("Arial", 12))
stationCost.pack(pady=4)

stationMaxChargeRate = tk.Label(root, text="Max Charge Rate: "+str(server_connection.station["maxChargeRate"])+" kW", font=("Arial", 12))
stationMaxChargeRate.pack(pady=4)

statusPort1 = tk.Label(root, text="Port 1: Available", fg="green", font=("Arial", 16))
statusPort1.pack(pady=8)

timePort1 = tk.Label(root, text="", font=("Arial", 12))
timePort1.pack(pady=8)

wattagePort1 = tk.Label(root, text="", fg="yellow", font=("Arial", 12))
wattagePort1.pack(pady=8)

statusPort2 = tk.Label(root, text="Port 2: Available", fg="green", font=("Arial", 16))
statusPort2.pack(pady=8)

timePort2 = tk.Label(root, text="", font=("Arial", 12))
timePort2.pack(pady=8)

wattagePort2 = tk.Label(root, text="", fg="yellow", font=("Arial", 12))
wattagePort2.pack(pady=8)

ads.config(frame,root)
ads_matrix.config(root)

last_status = [0,0]

def change_port_status(label, wattage_info, port_id, port_info):
    global last_status
    global buzzerIsOn

    if port_info["serviceMode"]:
        label.config(text="PORT "+str(port_id+1)+" IN SERVICE MODE.", fg="red")
        display.show_status(2,port_id)
    elif port_info["status"] == 0:
        wattage_info[0].config(text="")
        label.config(text="Port "+str(port_id+1)+": Available", fg="green")
        display.show_status(0,port_id)
    elif port_info["status"] == 1:
        display.show_status(1,port_id)
        label.config(text="Port "+str(port_id+1)+": Occupied", fg="yellow")
        wattage_info[0].config(text="Wattage "+str(wattage_info[1]))
    elif port_info["status"] == 2:
        label.config(text="PORT "+str(port_id+1)+" OUT OF SERVICE", fg="red")
        display.show_status(2,port_id)

    if port_info["status"] == 1 and last_status[port_id] == 0 and buzzerIsOn:
        GPIO.output(buzzer,GPIO.HIGH)
        sleep(0.5)
        GPIO.output(buzzer,GPIO.LOW)

    last_status[port_id] = port_info["status"]

def change_port_time(label, time, port_id):
    if time != None:
        label.config(text="Port "+str(port_id+1)+" Time: "+time)
    else:
        label.config(text="")

def loop():
    try:
        server_connection.get_port(0)
        server_connection.get_port(1)

        ports = server_connection.ports
        print(ports)

        change_port_time(timePort1, ports[0]["time"], 0)
        change_port_time(timePort2, ports[1]["time"], 1)

        wattages = charging_status.get_info()
        print(wattages)
        
        change_port_status(statusPort1, [wattagePort1, wattages[0]], 0, ports[0])
        change_port_status(statusPort2, [wattagePort2, wattages[1]], 1, ports[1])

        if GPIO.input(SWITCH_PIN) == GPIO.HIGH:
            wattages = [0,0]

        server_connection.set_wattage(0, wattages[0])
        server_connection.set_wattage(1, wattages[1])
    except:
        pass

    root.after(1000, loop)

# Start
loop()
root.mainloop()