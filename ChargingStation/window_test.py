import tkinter as tk
from PIL import Image, ImageTk
import os

root = tk.Tk()
root.title("GUI z obrazkami")

# --- Wczytanie ścieżek do obrazów ---
image_folder = "ads"
image_files = ["1.png", "2.png", "3.png"]
image_paths = [os.path.join(image_folder, name) for name in image_files]

current_image_index = 0

# --- Kontener główny: lewo/prawo ---
frame = tk.Frame(root)
frame.pack(fill=tk.BOTH, expand=True)

# --- Lewa część: tekst przycisku itp. ---
left_frame = tk.Frame(frame)
left_frame.pack(side=tk.LEFT, padx=10, pady=10)

label = tk.Label(left_frame, text="Stan: ???", font=("Arial", 24))
label.pack(pady=80)

# --- Prawa część: obrazki ---
right_frame = tk.Frame(frame)
right_frame.pack(side=tk.RIGHT, padx=10, pady=10)

image_label = tk.Label(right_frame)
image_label.pack()

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

update_image()  # start rotacji obrazków

root.mainloop()