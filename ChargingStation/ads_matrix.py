from PIL import Image
import display
import os
root = 0

current_image_index = 0

image_folder = "ads_matrix"
image_files = [f for f in os.listdir(image_folder) if f.lower().endswith(".bmp")]
image_paths = [os.path.join(image_folder, name) for name in image_files]

def config(rootMain):
    global root
    root = rootMain

    update_matrix()

def update_image_files():
    global image_files
    global image_paths
    image_files = [f for f in os.listdir(image_folder) if f.lower().endswith(".bmp")]
    image_paths = [os.path.join(image_folder, name) for name in image_files]

def update_matrix():
    update_image_files()

    global current_image_index
    path = image_paths[current_image_index]

    image = Image.open(path)
    width, height = image.size

    if image.mode != "RGB":
        image = image.convert("RGB")

    # Iteruj po każdym pikselu
    for y in range(height):
        for x in range(width):
            pixel = image.getpixel((x, y))

            print((pixel[0], pixel[1],pixel[2]))

            display.set_pixel(width-x-1,y,(pixel[0], pixel[1],pixel[2]))

    display.apply_changes()

    current_image_index = (current_image_index + 1) % len(image_paths)

    root.after(10000, update_matrix)
