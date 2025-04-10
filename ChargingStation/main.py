import RPi.GPIO as GPIO
import charging_station_ports
import time

GPIO.setmode(GPIO.BCM)  

charging_station_ports.turn_port_on(0)

time.sleep(1)

charging_station_ports.turn_port_on(1)

time.sleep(1)

charging_station_ports.turn_port_off(0)
charging_station_ports.turn_port_off(0)

# Cleanup
GPIO.cleanup()
