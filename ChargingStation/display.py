from main import Matrix, pixel_height, pixel_width, color_order

matrix = Matrix()

matrix = Matrix()


# Simple test to confirm color
# Red... Green... Blue... (Repeat)
def run(matrix, _):
    """red, green, blue, repeat"""
    while matrix.ready():
        matrix.reset(matrix.color("red"))
        matrix.show()
        matrix.delay(1000)

        matrix.reset(matrix.color("green"))
        matrix.show()
        matrix.delay(1000)

        matrix.reset(matrix.color("blue"))
        matrix.show()
        matrix.delay(1000)
run(
    matrix,
    {
        "pixel_height": pixel_height,
        "pixel_width": pixel_width,
        "color_order": color_order
    },
)
