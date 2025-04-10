import RPi.GPIO as GPIO

GPIO.setmode(GPIO.BCM)  

RELAY_LEFT_PIN = 23
RELAY_RIGHT_PIN = 24

pin_data = {
    0: RELAY_LEFT_PIN,
    1: RELAY_RIGHT_PIN
}

# Set up the GPIO pin
GPIO.setup(RELAY_LEFT_PIN, GPIO.OUT)
GPIO.setup(RELAY_RIGHT_PIN, GPIO.OUT)



def turn_port_on(port):
    print("Enabling port", port)
    GPIO.output(pin_data[port], GPIO.HIGH)

def turn_port_off(port):
    print("Disabling port", port)
    GPIO.output(pin_data[port], GPIO.LOW) 