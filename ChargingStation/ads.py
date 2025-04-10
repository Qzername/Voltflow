import tkinter as tk
from PIL import Image, ImageTk
import os

# --- Wczytanie ścieżek do obrazów ---
image_folder = "ads"
image_files = ["1.png", "2.png", "3.png"]
image_paths = [os.path.join(image_folder, name) for name in image_files]

current_image_index = 0

right_frame = 0
image_label = 0
root = 0

def config(frame, rootMain):
    global root
    root = rootMain
    
    # --- Prawa część: obrazki ---
    global right_frame

    right_frame = tk.Frame(frame)
    right_frame.pack(side=tk.RIGHT, padx=10, pady=10)

    global image_label

    image_label = tk.Label(right_frame)
    image_label.pack()
    
    update_image()  # start rotacji obrazków

# --- Funkcja do zmiany obrazka ---
def update_image():
    global current_image_index
    path = image_paths[current_image_index]
    
    # Wczytaj i przeskaluj obraz
    img = Image.open(path)
    img = img.resize((300, 200), Image.ANTIALIAS)  # dostosuj rozmiar według potrzeby
    photo = ImageTk.PhotoImage(img)
    
    # Ustaw w labelu
    image_label.config(image=photo)
    image_label.image = photo  # <- ważne: inaczej Python zgubi referencję!

    # Następny obrazek
    current_image_index = (current_image_index + 1) % len(image_paths)

    # Odpal ponownie za 10 sek.
    root.after(10000, update_image)

