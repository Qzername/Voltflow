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

# Clear display
for i in range(LED_COUNT):
    strip.setPixelColor(i, Color(0, 0, 0))
    
strip.show()

def set_pixel(x, y, color):
    if y % 2 == 0:
        i = y * LED_COLS + x
    else:
        i = y * LED_COLS + (LED_COLS - 1 - x)
    strip.setPixelColor(i, Color(color[0],color[1],color[2]))

def apply_changes():
    strip.show()

def show_status(status, port):
    color = (0,0,0)

    if status == 0:
        color = (0,100,0)
    elif status == 1:
        color = (100,100,0)
    elif status == 2:
        color = (100,0,0)

    start_X = 0

    if port == 1:
        start_X = 13
    
    for x in range(start_X, start_X+3,1):
        for y in range(0,3):
            set_pixel(x,y,color)

    strip.show()