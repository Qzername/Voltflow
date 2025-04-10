from main import Matrix

matrix = Matrix()

def test_matrix():
    matrix.reset()
    matrix.line((0,0), (16,16), (255,0,0),1)
    matrix.show()
    matrix.delay()

test_matrix()