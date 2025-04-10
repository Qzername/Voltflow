import RPi.GPIO as GPIO
import time

SWITCH_PIN = 26  # Use BCM numbering

GPIO.setmode(GPIO.BCM)
GPIO.setup(SWITCH_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)  # Pull-up

def switch_callback(channel):
    if GPIO.input(channel) == GPIO.LOW:
        print("Switch pressed")
    else:
        print("Switch released")

# Add interrupt detection
GPIO.remove_event_detect(SWITCH_PIN)
GPIO.add_event_detect(SWITCH_PIN, GPIO.BOTH, callback=switch_callback, bouncetime=200)

try:
    while True:
        time.sleep(1)
except KeyboardInterrupt:
    GPIO.cleanup()