import os
import numpy as np

class Vector3:
    def __init__(self, x=0, y=0, z=0):
        self.x = x
        self.y = y
        self.z = z

class Triangle:
    def __init__(self, indices):
        self.indices = indices

gPositions = []
gNormals = []
gTriangles = []

def tokenize(string, delimiter):
    tokens = string.split(delimiter)
    return tokens

def face_index(string):
    tokens = tokenize(string, "/")
    if tokens[0] and tokens[-1] and int(tokens[0]) == int(tokens[-1]):
        return int(tokens[0])
    else:
        print("ERROR: Bad face specifier!")
        exit(0)

def load_mesh(fileName):
    if not os.path.isfile(fileName):
        print(f"ERROR: Unable to load mesh from {fileName}!")
        exit(0)

    xmin, ymin, zmin = float('inf'), float('inf'), float('inf')
    xmax, ymax, zmax = float('-inf'), float('-inf'), float('-inf')

    with open(fileName, 'r') as fin:
        for line in fin:
            if line.startswith('v '):
                tokens = tokenize(line[2:], ' ')
                x, y, z = float(tokens[0]), float(tokens[1]), float(tokens[2])
                gPositions.append(Vector3(x, y, z))
                xmin, ymin, zmin = min(xmin, x), min(ymin, y), min(zmin, z)
                xmax, ymax, zmax = max(xmax, x), max(ymax, y), max(zmax, z)
            elif line.startswith('vn '):
                tokens = tokenize(line[3:], ' ')
                x, y, z = float(tokens[0]), float(tokens[1]), float(tokens[2])
                gNormals.append(Vector3(x, y, z))
            elif line.startswith('f '):
                tokens = tokenize(line[2:], ' ')
                indices = [face_index(t) - 1 for t in tokens[:3]]  # Subtracting 1 to convert to 0-based index
                gTriangles.append(Triangle(indices))

    print(f"Mesh loaded with {len(gPositions)} vertices, {len(gNormals)} normals, and {len(gTriangles)} triangles.")
    print(f"Bounding box: xmin={xmin}, ymin={ymin}, zmin={zmin}, xmax={xmax}, ymax={ymax}, zmax={zmax}")

# Example usage
load_mesh("bunny.obj")

