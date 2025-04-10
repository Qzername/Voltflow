from rpi_ws281x import PixelStrip, Color
import time

# LED matrix configuration
LED_ROWS = 16
LED_COLS = 16
LED_COUNT = LED_ROWS * LED_COLS
LED_PIN = 18  # GPIO18 (PWM)

# Create PixelStrip object
strip = PixelStrip(LED_COUNT, LED_PIN)
strip.begin()

def set_pixel(x, y, color):
    if y % 2 == 0:
        i = y * LED_COLS + x
    else:
        i = y * LED_COLS + (LED_COLS - 1 - x)
    strip.setPixelColor(i, color)

# Fill entire matrix red
for y in range(LED_ROWS):
    for x in range(LED_COLS):
        set_pixel(x, y, Color(255, 0, 0))

strip.show()
time.sleep(1)

# Clear display
for i in range(LED_COUNT):
    strip.setPixelColor(i, Color(0, 0, 0))
strip.show()
