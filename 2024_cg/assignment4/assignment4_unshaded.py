import numpy as np
from math import pi, sin, cos
from PIL import Image

def create_sphere(width=32, height=16):
    num_vertices = (height - 2) * width + 2
    num_triangles = (height - 2) * (width - 1) * 2
    
    vertices = np.zeros((num_vertices, 3))  # x, y, z for each vertex
    index_buffer = np.zeros((num_triangles * 3), dtype=int)

    # Generate vertices on the sphere surface
    t = 0
    for j in range(1, height-1):
        for i in range(width):
            theta = j / (height-1) * pi
            phi = i / (width-1) * 2 * pi
            
            x = sin(theta) * cos(phi)
            y = cos(theta)
            z = -sin(theta) * sin(phi)
            vertices[t] = [x, y, z]
            t += 1

    # North pole
    vertices[t] = [0, 1, 0]  # Top pole
    north_pole_index = t
    t += 1

    # South pole
    vertices[t] = [0, -1, 0]  # Bottom pole
    south_pole_index = t

    # Generate index buffer for the mesh triangles
    t = 0
    for j in range(height-3):
        for i in range(width-1):
            index_buffer[t:t+3] = [j*width + i, (j+1)*width + (i+1), j*width + (i+1)]
            t += 3
            index_buffer[t:t+3] = [j*width + i, (j+1)*width + i, (j+1)*width + (i+1)]
            t += 3

    # Cap triangles
    for i in range(width-1):
        index_buffer[t:t+3] = [north_pole_index, i, i+1]
        t += 3
        index_buffer[t:t+3] = [south_pole_index, (height-3)*width + i, (height-3)*width + (i+1)]
        t += 3

    return vertices, index_buffer.reshape((-1, 3))

def transform_sphere(vertices, scale=2, translate=(0, 0, -7)):
    transformed_vertices = np.copy(vertices)
    for i in range(len(vertices)):
        # Apply scaling
        transformed_vertices[i] *= scale
        # Apply translation
        transformed_vertices[i] += np.array(translate)
    return transformed_vertices

def barycentric_coords(p, a, b, c):
    # Compute vectors for barycentric coordinates
    v0 = b - a
    v1 = c - a
    v2 = p - a
    d00 = np.dot(v0, v0)
    d01 = np.dot(v0, v1)
    d11 = np.dot(v1, v1)
    d20 = np.dot(v2, v0)
    d21 = np.dot(v2, v1)
    denom = d00 * d11 - d01 * d01
    
    # Check if the denominator is near zero (degenerate triangle)
    if abs(denom) < 1e-10:
        return -1, -1, -1  # Invalid coordinates signify point outside triangle or degenerate case

    v = (d11 * d20 - d01 * d21) / denom
    w = (d00 * d21 - d01 * d20) / denom
    u = 1.0 - v - w
    return u, v, w


def perspective_projection(vertices, l, r, b, t, n, f):
    # Perspective projection matrix
    P = np.array([
        [2 * n / (r - l), 0, (r + l) / (r - l), 0],
        [0, 2 * n / (t - b), (t + b) / (t - b), 0],
        [0, 0, -(f + n) / (f - n), -2 * f * n / (f - n)],
        [0, 0, -1, 0]
    ])

    # Apply perspective projection
    homogeneous_vertices = np.column_stack((vertices, np.ones(vertices.shape[0])))
    projected_vertices = np.dot(homogeneous_vertices, P.T)

    # Normalize by dividing by homogeneous coordinate
    projected_vertices /= projected_vertices[:, 3][:, np.newaxis]

    # Remove homogeneous coordinate
    projected_vertices = projected_vertices[:, :3]

    return projected_vertices

def viewport_transform(vertices, nx, ny, near, far):
    # Viewport transformation matrix
    V = np.array([
        [nx / 2, 0, 0, (nx - 1) / 2],
        [0, ny / 2, 0, (ny - 1) / 2],
        [0, 0, (far - near) / 2, (far + near) / 2],
        [0, 0, 0, 1]
    ])

    # Apply viewport transformation
    homogeneous_vertices = np.column_stack((vertices, np.ones(vertices.shape[0])))
    transformed_vertices = np.dot(homogeneous_vertices, V.T)

    # Remove homogeneous coordinate
    transformed_vertices = transformed_vertices[:, :3]

    return transformed_vertices

def rasterize_triangles(vertices, triangles, image_size=(512, 512)):
    image = Image.new("RGB", image_size, "black")
    pixels = image.load()
    z_buffer = np.full(image_size, np.inf)
    
    for triangle in triangles:
        v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
        
        # Triangle bounding box
        xmin = max(min(v0[0], v1[0], v2[0]), 0)
        xmax = min(max(v0[0], v1[0], v2[0]), image_size[0] - 1)
        ymin = max(min(v0[1], v1[1], v2[1]), 0)
        ymax = min(max(v0[1], v1[1], v2[1]), image_size[1] - 1)
        
        for x in range(int(xmin), int(xmax) + 1):
            for y in range(int(ymin), int(ymax) + 1):
                u, v, w = barycentric_coords(np.array([x, y]), v0[:2], v1[:2], v2[:2])
                if u >= 0 and v >= 0 and w >= 0:  # Point inside the triangle
                    pixels[x, y] = (255, 255, 255) #(int(255 * u), int(255 * v), int(255 * w))
                    
                    z = u * v0[2] + v * v1[2] + w * v2[2]
                    if z_buffer[y, x] > z:
                        z_buffer[y, x] = z
                        
    image.show()
    return image

# Create sphere data
vertices, triangles = create_sphere()
transformed_vertices = transform_sphere(vertices)

# Apply perspective projection transform
transformed_vertices = perspective_projection(transformed_vertices, -0.1, 0.1, -0.1, 0.1, -0.1, -1000)

# Apply viewport transform
transformed_vertices = viewport_transform(transformed_vertices, 512, 512, -0.1, -1000)

# Rasterize triangles
image = rasterize_triangles(transformed_vertices, triangles, image_size=(512, 512))
image.save("rasterized_sphere.png")  # Save the image file
