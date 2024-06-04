import sys
import os
import numpy as np
from OpenGL.GL import *
from OpenGL.GLUT import *
from OpenGL.GLU import *

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

    with open(fileName, 'r') as fin:
        for line in fin:
            if line.startswith('v '):
                tokens = tokenize(line[2:], ' ')
                x, y, z = float(tokens[0]), float(tokens[1]), float(tokens[2])
                gPositions.append(Vector3(x, y, z))
            elif line.startswith('vn '):
                tokens = tokenize(line[3:], ' ')
                x, y, z = float(tokens[0]), float(tokens[1]), float(tokens[2])
                gNormals.append(Vector3(x, y, z))
            elif line.startswith('f '):
                tokens = tokenize(line[2:], ' ')
                indices = [face_index(t) - 1 for t in tokens[:3]]  # Subtracting 1 to convert to 0-based index
                gTriangles.append(Triangle(indices))

    print(f"Mesh loaded with {len(gPositions)} vertices, {len(gNormals)} normals, and {len(gTriangles)} triangles.")

vao = None
vbo_positions = None
vbo_normals = None
ebo = None

def init_buffers():
    global vao, vbo_positions, vbo_normals, ebo

    positions = np.array([[p.x, p.y, p.z] for p in gPositions], dtype=np.float32)
    normals = np.array([[n.x, n.y, n.z] for n in gNormals], dtype=np.float32)
    indices = np.array([index for tri in gTriangles for index in tri.indices], dtype=np.uint32)

    vao = glGenVertexArrays(1)
    glBindVertexArray(vao)

    vbo_positions = glGenBuffers(1)
    glBindBuffer(GL_ARRAY_BUFFER, vbo_positions)
    glBufferData(GL_ARRAY_BUFFER, positions.nbytes, positions, GL_STATIC_DRAW)
    glEnableVertexAttribArray(0)
    glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, 0, None)

    vbo_normals = glGenBuffers(1)
    glBindBuffer(GL_ARRAY_BUFFER, vbo_normals)
    glBufferData(GL_ARRAY_BUFFER, normals.nbytes, normals, GL_STATIC_DRAW)
    glEnableVertexAttribArray(1)
    glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, 0, None)

    ebo = glGenBuffers(1)
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo)
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, indices.nbytes, indices, GL_STATIC_DRAW)

    glBindVertexArray(0)

def init():
    glEnable(GL_DEPTH_TEST)        
    glDisable(GL_CULL_FACE)  # Back-face culling 비활성화

    # Material
    ka = [1.0, 1.0, 1.0, 1.0]
    kd = [1.0, 1.0, 1.0, 1.0]
    ks = [0.0, 0.0, 0.0, 1.0]
    p = 0.0
    
    glMaterialfv(GL_FRONT, GL_AMBIENT, ka)
    glMaterialfv(GL_FRONT, GL_DIFFUSE, kd)
    glMaterialfv(GL_FRONT, GL_SPECULAR, ks)
    glMaterialf(GL_FRONT, GL_SHININESS, p)


    # Light
    glEnable(GL_LIGHTING)
    glEnable(GL_LIGHT0)

    # Ambient light intensity
    ambient_light_intensity = [0.2, 0.2, 0.2, 1.0]
    glLightModelfv(GL_LIGHT_MODEL_AMBIENT, ambient_light_intensity)

    # Directional light source
    light_ambient = [0.0, 0.0, 0.0, 1.0]
    light_diffuse = [1.0, 1.0, 1.0, 1.0]
    light_specular = [0.0, 0.0, 0.0, 1.0]
    light_direction = [-1.0, -1.0, -1.0, 0.0]

    glEnable(GL_LIGHT1)
    glLightfv(GL_LIGHT1, GL_AMBIENT, light_ambient)
    glLightfv(GL_LIGHT1, GL_DIFFUSE, light_diffuse)
    glLightfv(GL_LIGHT1, GL_SPECULAR, light_specular)
    glLightfv(GL_LIGHT1, GL_SPOT_DIRECTION, light_direction)

    init_buffers()

def display():
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glLoadIdentity()
    gluLookAt(0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, 1.0, 0.0);

    glPushMatrix()
    glTranslatef(0.1, -1.0, -1.5)
    glScalef(10.0, 10.0, 10.0)

    glBindVertexArray(vao)
    glDrawElements(GL_TRIANGLES, len(gTriangles) * 3, GL_UNSIGNED_INT, None)
    glBindVertexArray(0)

    glPopMatrix()
    glutSwapBuffers()

def reshape(width, height):
    glViewport(0, 0, width, height)
    glMatrixMode(GL_PROJECTION)
    glLoadIdentity()
    glFrustum(-0.1, 0.1, -0.1, 0.1, 0.1, 1000.0)
    glMatrixMode(GL_MODELVIEW)
    glLoadIdentity()

def main():
    load_mesh("bunny.obj")

    glutInit(sys.argv)
    glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB | GLUT_DEPTH)
    glutInitWindowSize(512, 512)
    glutCreateWindow("Bunny Renderer - Vertex Array Object")

    init()

    glutDisplayFunc(display)
    glutReshapeFunc(reshape)
    glutMainLoop()

if __name__ == "__main__":
    main()
