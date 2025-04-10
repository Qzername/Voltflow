import RPi.GPIO as GPIO
import time

RELAY_LEFT_PIN = 14
RELAY_RIGHT_PIN = 15

# Set the GPIO mode
GPIO.setmode(GPIO.BCM)  # Use BCM numbering (GPIO numbers)

# Set up the GPIO pin
GPIO.setup(RELAY_LEFT_PIN, GPIO.OUT)

# Turn relay ON
GPIO.output(RELAY_LEFT_PIN, GPIO.LOW)  # Some relays are active-low
print("Relay ON")
time.sleep(2)

# Turn relay OFF
GPIO.output(RELAY_LEFT_PIN, GPIO.HIGH)
print("Relay OFF")
time.sleep(2)

# Cleanup
GPIO.cleanup()

def turn_port_on(port):
    pass

def turn_port_off(port):
    pass