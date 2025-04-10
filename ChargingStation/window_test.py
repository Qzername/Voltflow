import tkinter as tk

# Tworzymy główne okno
root = tk.Tk()
root.title("Moje okno")

# Tworzymy płótno do rysowania
canvas = tk.Canvas(root, width=400, height=300, bg="white")
canvas.pack()

# Rysujemy linię
canvas.create_line(50, 50, 200, 50, fill="blue", width=3)

# Rysujemy okrąg (oval w prostokącie)
canvas.create_oval(100, 100, 200, 200, outline="red", fill="pink", width=2)

# Rysujemy tekst
canvas.create_text(200, 250, text="Hej!", font=("Arial", 20), fill="green")

# Uruchamiamy pętlę główną
root.mainloop()