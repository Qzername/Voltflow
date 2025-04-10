import RPi.GPIO as GPIO
import charging_station_ports
import server_connection
import display

server_connection.init()

GPIO.setmode(GPIO.BCM)

charging_station_ports.turn_port_off(0)
charging_station_ports.turn_port_off(1)

while True:
    pass