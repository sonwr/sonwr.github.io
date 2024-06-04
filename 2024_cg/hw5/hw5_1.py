import sys
import os
import time
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

class FPSCounter:
    def __init__(self):
        self.start_time = time.time()
        self.frame_count = 0
        self.fps = 0

    def update(self):
        self.frame_count += 1
        current_time = time.time()
        elapsed_time = current_time - self.start_time

        if elapsed_time > 1.0:
            self.fps = self.frame_count / elapsed_time
            self.start_time = current_time
            self.frame_count = 0

    def get_fps(self):
        return self.fps

fps_counter = FPSCounter()

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

def draw_text(x, y, text):
    glWindowPos2f(x, y)
    for ch in text:
        glutBitmapCharacter(GLUT_BITMAP_HELVETICA_18, ord(ch))

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

def display():
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
    glLoadIdentity()
    gluLookAt(0.0, 0.0, 0.0, 0.0, 0.0, -1.0, 0.0, 1.0, 0.0)

    glPushMatrix()
    glTranslatef(0.1, -1.0, -1.5)
    glScalef(10.0, 10.0, 10.0)

    glBegin(GL_TRIANGLES)
    for triangle in gTriangles:
        for i in range(3):
            index = triangle.indices[i]
            normal = gNormals[index]
            position = gPositions[index]
            glNormal3f(normal.x, normal.y, normal.z)
            glVertex3f(position.x, position.y, position.z)
    glEnd()

    glPopMatrix()

    fps_counter.update()
    fps_text = f"FPS: {fps_counter.get_fps():.2f}"
    draw_text(10, 10, fps_text)
    

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
    glutCreateWindow("Bunny Renderer - Immediate Mode")

    init()

    glutDisplayFunc(display)
    glutReshapeFunc(reshape)
    glutIdleFunc(display)
    glutMainLoop()

if __name__ == "__main__":
    main()
