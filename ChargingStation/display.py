from main import Matrix

matrix = Matrix()

def test_matrix():
    matrix.reset()  # black background (0, 0, 0)
    matrix.line((0, 0), (60, 30), (255, 0, 0), 1)  # diagonal red line
    matrix.rectangle((5, 5), (55, 25), (0, 255, 0), 1)  # green rectangle
    matrix.circle((30, 15), 10, (0, 0, 255), 2)  # blue circle
    matrix.show()
    matrix.delay(10000) 

test_matrix()