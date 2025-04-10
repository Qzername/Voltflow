from PIL import Image
import display

# Otwórz obraz BMP
image = Image.open("ads_matrix/1.bmp")
width, height = image.size

# Iteruj po każdym pikselu
for y in range(height):
    for x in range(width):
        pixel = image.getpixel((x, y))

        print(f"Piksel ({x}, {y}): {pixel}")
        
        display.set_pixel(x,y,(pixel[0], pixel[1],pixel[2]))

display.apply_changes()