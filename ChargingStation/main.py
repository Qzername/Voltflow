import RPi.GPIO as GPIO
import charging_station_ports
import server_connection
import display

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

while True:
    pass