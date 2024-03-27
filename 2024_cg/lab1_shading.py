import numpy as np
from PIL import Image

# Scene configuration
image_width, image_height = 512, 512
camera_position = np.array([0, 0, 0])
viewing_plane_distance = 0.1
l, r, b, t = -0.1, 0.1, -0.1, 0.1
ambient_intensity = np.array([0.1, 0.1, 0.1])
gamma = 2.2
light_position = np.array([-4, 4, -3])
light_intensity = np.array([1, 1, 1])

# Initialize the image
image = np.zeros((image_height, image_width, 3), dtype=np.float32)

# Material class
class Material:
    def __init__(self, ka, kd, ks, specular_power):
        self.ka = np.array(ka)
        self.kd = np.array(kd)
        self.ks = np.array(ks)
        self.specular_power = specular_power

# Sphere class
class Sphere:
    def __init__(self, center, radius, material):
        self.center = np.array(center)
        self.radius = radius
        self.material = material

    def intersect(self, ray_origin, ray_direction):
        oc = ray_origin - self.center
        a = np.dot(ray_direction, ray_direction)
        b = 2.0 * np.dot(oc, ray_direction)
        c = np.dot(oc, oc) - self.radius * self.radius
        discriminant = b * b - 4 * a * c
        if discriminant > 0:
            sqrt_discriminant = np.sqrt(discriminant)
            t1 = (-b - sqrt_discriminant) / (2.0 * a)
            t2 = (-b + sqrt_discriminant) / (2.0 * a)
            if t1 > 0 and t2 > 0:
                return min(t1, t2), self
        return float('inf'), None

    def normal_at(self, point):
        return (point - self.center) / self.radius

# Plane class
class Plane:
    def __init__(self, point, normal, material):
        self.point = np.array(point)
        self.normal = np.array(normal)
        self.material = material

    def intersect(self, ray_origin, ray_direction):
        denominator = np.dot(ray_direction, self.normal)
        if np.abs(denominator) > 1e-6:
            t = np.dot(self.point - ray_origin, self.normal) / denominator
            if t > 0:
                return t, self
        return float('inf'), None

    def normal_at(self, _):
        return self.normal

# Scene setup
materials = {
    'plane': Material(ka=(0.2, 0.2, 0.2), kd=(1, 1, 1), ks=(0, 0, 0), specular_power=0),
    'sphere1': Material(ka=(0.2, 0, 0), kd=(1, 0, 0), ks=(0, 0, 0), specular_power=0),
    'sphere2': Material(ka=(0, 0.2, 0), kd=(0, 0.5, 0), ks=(0.5, 0.5, 0.5), specular_power=32),
    'sphere3': Material(ka=(0, 0, 0.2), kd=(0, 0, 1), ks=(0, 0, 0), specular_power=0),
}

objects = [
    Plane(point=[0, -2, 0], normal=[0, 1, 0], material=materials['plane']),
    Sphere(center=[-4, 0, -7], radius=1, material=materials['sphere1']),
    Sphere(center=[0, 0, -7], radius=2, material=materials['sphere2']),
    Sphere(center=[4, 0, -7], radius=1, material=materials['sphere3']),
]

# Function to trace a ray and find the closest intersection
def trace(ray_origin, ray_direction):
    closest_t = float('inf')
    closest_object = None
    for obj in objects:
        t, obj_hit = obj.intersect(ray_origin, ray_direction)
        if t and t < closest_t:
            closest_t = t
            closest_object = obj_hit
   
    return closest_t, closest_object

# Function to compute the color at the intersection point
def compute_color(ray_origin, ray_direction, closest_object, closest_t):
    hit_point = ray_origin + closest_t * ray_direction
    normal = closest_object.normal_at(hit_point)
    view_direction = -ray_direction
    light_direction = light_position - hit_point
    light_distance = np.linalg.norm(light_direction)
    light_direction /= light_distance

    # Shadow check
    shadow_t, shadow_object = trace(hit_point + normal * 1e-5, light_direction)
    in_shadow = shadow_t < light_distance

    # Ambient component
    ambient = closest_object.material.ka * ambient_intensity

    if not in_shadow:
        # Diffuse component
        diffuse_intensity = np.dot(light_direction, normal)
        diffuse_intensity = max(0, diffuse_intensity)
        diffuse = closest_object.material.kd * light_intensity * diffuse_intensity

        # Specular component
        reflect_direction = 2 * normal * np.dot(light_direction, normal) - light_direction
        specular_intensity = np.dot(reflect_direction, view_direction)
        specular_intensity = max(0, specular_intensity) ** closest_object.material.specular_power
        specular = closest_object.material.ks * light_intensity * specular_intensity

        # Sum up components
        color = ambient + diffuse + specular
    else:
        color = ambient

    # Gamma correction
    color = np.clip(color, 0, 1)
    color = np.power(color, 1/gamma)

    return color

# Rendering loop
for x in range(image_width):
    for y in range(image_height):
        # Convert pixel coordinate to world coordinate
        u = l + (r - l) * (x + 0.5) / image_width
        v = b + (t - b) * (y + 0.5) / image_height
        ray_direction = np.array([u, v, -viewing_plane_distance])
        ray_direction = ray_direction / np.linalg.norm(ray_direction)

        closest_t, closest_object = trace(camera_position, ray_direction)
        if closest_object is not None:
            color = compute_color(camera_position, ray_direction, closest_object, closest_t)
            image[y, x] = color * 255
        else:
            image[y, x] = [0, 0, 0] # Background color

image = np.flipud(image)

# Convert the float image array to uint8
image = image.astype(np.uint8)

# Save the image
img = Image.fromarray(image, 'RGB')
img_path = "ray_tracing_phong_shading.png"
img.save(img_path)
img.show()
