import RPi.GPIO as GPIO
import time

SWITCH_PIN = 26  # Use BCM numbering

GPIO.setmode(GPIO.BCM)
GPIO.setup(SWITCH_PIN, GPIO.IN, pull_up_down=GPIO.PUD_UP)  # Pull-up

try:
    while True:
        if GPIO.input(SWITCH_PIN) == GPIO.LOW:
            print("Switch pressed")
        else:
            print("Switch released")
        time.sleep(1)
except KeyboardInterrupt:
    GPIO.cleanup()