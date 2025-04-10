import RPi.GPIO as GPIO

RELAY_LEFT_PIN = 14
RELAY_RIGHT_PIN = 15

pin_data = {
    0: RELAY_LEFT_PIN,
    1: RELAY_RIGHT_PIN
}

# Set up the GPIO pin
GPIO.setup(RELAY_LEFT_PIN, GPIO.OUT)
GPIO.setup(RELAY_RIGHT_PIN, GPIO.OUT)

def turn_port_on(port):
    GPIO.output(pin_data[port], GPIO.HIGH)

def turn_port_off(port):
    GPIO.output(pin_data[port], GPIO.LOW) 