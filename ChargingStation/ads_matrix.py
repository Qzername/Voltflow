from PIL import Image
import display

# Otwórz obraz BMP
image = Image.open("ads_matrix/1.bmp")
width, height = image.size

if image.mode != "RGB":
    image = image.convert("RGB")

print(image.mode)
print(height)
print(width)

# Iteruj po każdym pikselu
for y in range(height):
    for x in range(width):
        pixel = image.getpixel((x, y))

        print(f"Piksel ({x}, {y}): {pixel}")
        
        display.set_pixel(width-x-1,y,(pixel[0], pixel[2],pixel[1]))

display.apply_changes()