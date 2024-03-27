import numpy as np
from PIL import Image

# Scene settings
image_width, image_height = 512, 512
camera_position = np.array([0, 0, 0])
viewing_plane_distance = 0.1
l, r, b, t = -0.1, 0.1, -0.1, 0.1

# Image buffer
image = np.zeros((image_height, image_width, 3), dtype=np.uint8)

# Sphere and Plane definitions
class Sphere:
    def __init__(self, center, radius):
        self.center = np.array(center)
        self.radius = radius

    def intersect(self, ray_origin, ray_direction):
        b = 2 * np.dot(ray_direction, ray_origin - self.center)
        c = np.linalg.norm(ray_origin - self.center) ** 2 - self.radius ** 2
        delta = b**2 - 4 * c
        if delta > 0:
            dist = (-b - np.sqrt(delta)) / 2
            if dist > 0:
                return True
        return False

class Plane:
    def __init__(self, point, normal):
        self.point = np.array(point)
        self.normal = np.array(normal)

    def intersect(self, ray_origin, ray_direction):
        denom = np.dot(self.normal, ray_direction)
        if np.abs(denom) > 1e-6:
            t = np.dot(self.point - ray_origin, self.normal) / denom
            return t > 0
        return False

# Objects in the scene
objects = [
    Plane(point=[0, -2, 0], normal=[0, 1, 0]),
    Sphere(center=[-4, 0, -7], radius=1),
    Sphere(center=[0, 0, -7], radius=2),
    Sphere(center=[4, 0, -7], radius=1)
]

# Ray tracing
for x in range(image_width):
    for y in range(image_height):
        # Convert pixel coordinate to world coordinate
        u = l + (r - l) * (x + 0.5) / image_width
        v = b + (t - b) * (y + 0.5) / image_height
        ray_direction = np.array([u, v, -viewing_plane_distance])
        ray_direction = ray_direction / np.linalg.norm(ray_direction)

        # Check intersection with each object
        hit_anything = any(obj.intersect(camera_position, ray_direction) for obj in objects)
        
        # Color the pixel
        image[y, x] = [255, 255, 255] if hit_anything else [0, 0, 0]

# Save the image
image = np.flipud(image)
img = Image.fromarray(image, 'RGB')
img.save('ray_tracing_result.png')
