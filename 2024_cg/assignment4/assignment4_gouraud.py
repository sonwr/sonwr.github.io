import numpy as np
import math
from math import pi, sin, cos
from PIL import Image

gNumVertices = 0  # Number of 3D vertices.
gNumTriangles = 0  # Number of triangles.
gIndexBuffer = None  # Vertex indices for the triangles.

def create_scene():
    global gNumVertices, gNumTriangles, gIndexBuffer
    
    width = 32
    height = 16

    theta, phi = 0.0, 0.0
    t = 0

    gNumVertices = (height - 2) * width + 2
    gNumTriangles = (height - 2) * (width - 1) * 2

    # Allocate an array for gNumVertices vertices.
    vertices = np.zeros((gNumVertices, 3), dtype=np.float32)

    gIndexBuffer = np.zeros(3 * gNumTriangles, dtype=np.int32)

    t = 0
    for j in range(1, height - 1):
        for i in range(width):
            theta = j / (height - 1) * math.pi
            phi = i / (width - 1) * math.pi * 2

            x = math.sin(theta) * math.cos(phi)
            y = math.cos(theta)
            z = 2* -math.sin(theta) * math.sin(phi)

            # Set vertex t in the vertex array to {x, y, z}.
            vertices[t] = [x, y, z]

            t += 1

    # Set vertex t in the vertex array to {0, 1, 0}.
    vertices[t] = [0, 1, 0]
    t += 1

    # Set vertex t in the vertex array to {0, -1, 0}.
    vertices[t] = [0, -1, 0]
    t += 1

    t = 0
    for j in range(height - 3):
        for i in range(width - 1):
            gIndexBuffer[t] = j * width + i
            gIndexBuffer[t + 1] = (j + 1) * width + (i + 1)
            gIndexBuffer[t + 2] = j * width + (i + 1)
            gIndexBuffer[t + 3] = j * width + i
            gIndexBuffer[t + 4] = (j + 1) * width + i
            gIndexBuffer[t + 5] = (j + 1) * width + (i + 1)
            t += 6

    for i in range(width - 1):
        gIndexBuffer[t] = (height - 2) * width
        gIndexBuffer[t + 1] = i
        gIndexBuffer[t + 2] = i + 1
        gIndexBuffer[t + 3] = (height - 2) * width + 1
        gIndexBuffer[t + 4] = (height - 3) * width + (i + 1)
        gIndexBuffer[t + 5] = (height - 3) * width + i
        t += 6

    #print(gIndexBuffer[:10])
    return vertices, gIndexBuffer.reshape((-1, 3))

def transform_sphere(vertices, scale=2, translate=(0, 0, -7)):
    transformed_vertices = np.copy(vertices)
    for i in range(len(vertices)):
        # Apply scaling
        transformed_vertices[i] *= scale
        # Apply translation
        transformed_vertices[i] += np.array(translate)
    return transformed_vertices


def perspective_projection(vertices, l, r, b, t, n, f):
    # Perspective projection matrix
    _P = np.array([
        [(2 * n) / (r - l), 0, (r + l) / (r - l), 0],
        [0, (2 * n) / (t - b), (t + b) / (t - b), 0],
        [0, 0, -(f + n) / (f - n), -(2 * f * n) / (f - n)],
        [0, 0, -1, 0]
    ])

    P = np.array([
        [(2 * n) / (r - l), 0, (l + r) / (l - r), 0],
        [0, (2 * n) / (t - b), (b + t) / (b - t), 0],
        [0, 0, (f + n) / (n - f), (2 * f * n) / (f - n)],
        [0, 0, 1, 0]
    ])

    # Apply perspective projection
    homogeneous_vertices = np.column_stack((vertices, np.ones(vertices.shape[0])))
    projected_vertices = np.dot(homogeneous_vertices, P.T)

    # Normalize by dividing by homogeneous coordinate
    projected_vertices /= projected_vertices[:, 3][:, np.newaxis]

    # Remove homogeneous coordinate
    projected_vertices = projected_vertices[:, :3]

    return projected_vertices

def viewport_transform(vertices, nx, ny):
    # Viewport transformation matrix
    V = np.array([
        [nx / 2, 0, 0, (nx - 1) / 2],
        [0, ny / 2, 0, (ny - 1) / 2],
        [0, 0, nx + ny, 0],     # TODO: z-axis range
        [0, 0, 0, 1]
    ])

    # Apply viewport transformation
    homogeneous_vertices = np.column_stack((vertices, np.ones(vertices.shape[0])))
    transformed_vertices = np.dot(homogeneous_vertices, V.T)

    # Remove homogeneous coordinate
    transformed_vertices = transformed_vertices[:, :3]

    return transformed_vertices

def combine_transformations(scale, translate, l, r, b, t, n, f, nx, ny):
    # Scale and translate transformation matrix
    S = np.array([
        [scale, 0, 0, translate[0]],
        [0, scale, 0, translate[1]],
        [0, 0, scale, translate[2]],
        [0, 0, 0, 1]
    ])

    # Perspective projection matrix
    P = np.array([
        [(2 * n) / (r - l), 0, (l + r) / (l - r), 0],
        [0, (2 * n) / (t - b), (b + t) / (b - t), 0],
        [0, 0, (f + n) / (n - f), (2 * f * n) / (f - n)],
        [0, 0, 1, 0]
    ])

    # Viewport transformation matrix
    V = np.array([
        [nx / 2, 0, 0, (nx - 1) / 2],
        [0, ny / 2, 0, (ny - 1) / 2],
        [0, 0, nx + ny, 0],     # TODO: z-axis range
        [0, 0, 0, 1]
    ])
    

    #combined_matrix = V @ P @ S 
    combined_matrix = V * P * S
    
    return combined_matrix


def apply_transformation_matrix(vertices, combined_matrix):
    homogeneous_vertices = np.column_stack((vertices, np.ones(vertices.shape[0])))
    transformed_vertices = np.dot(homogeneous_vertices, combined_matrix.T)

    transformed_vertices = transformed_vertices[:, :3]

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


def calculate_triangle_normal(vertices, triangle):
    v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
    edge1 = v1 - v0
    edge2 = v2 - v0
    normal = np.cross(edge1, edge2)

    #print("[idx] x y z: ", triangle[0], triangle[1], triangle[2])
    #print("[val] x y z: ", vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]])

    return (normal / np.linalg.norm(normal))   # TODO: normal 반대?

def calculate_camera_direction():
    # 카메라 방향은 -z 축을 따라 향하는 단위 벡터로 가정합니다.
    camera_direction = np.array([0, 0, -1])
    return camera_direction

def gouraud_shading(vertices, triangles, image_size=(512, 512)):
    image = Image.new("RGB", image_size, "black")
    pixels = image.load()

    light_pos = np.array([-4, 4, -3])
    light_intensity = {'ambient': 0.2, 'point': 1.0}
    material = {'ka': np.array([0, 1, 0]), 'kd': np.array([0, 0.5, 0]), 'ks': np.array([0.5, 0.5, 0.5]), 'p': 32}

    vertex_colors = {}

    # Calculate color at each vertex
    for i in range(len(vertices)):
        normal = calculate_normal_at_vertex(vertices, triangles, i)
        color = calculate_lighting(normal, light_pos, vertices[i], material, light_intensity)
        vertex_colors[i] = np.clip(color, 0, 255).astype(int)  # Ensure the color values are within RGB bounds

    for triangle in triangles:
        v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
        c0, c1, c2 = vertex_colors[triangle[0]], vertex_colors[triangle[1]], vertex_colors[triangle[2]]

        # Triangle bounding box for rasterization
        xmin = max(min(v0[0], v1[0], v2[0]), 0)
        xmax = min(max(v0[0], v1[0], v2[0]), image_size[0] - 1)
        ymin = max(min(v0[1], v1[1], v2[1]), 0)
        ymax = min(max(v0[1], v1[1], v2[1]), image_size[1] - 1)

        for x in range(int(xmin), int(xmax) + 1):
            for y in range(int(ymin), int(ymax) + 1):
                u, v, w = barycentric_coords(np.array([x, y]), v0[:2], v1[:2], v2[:2])
                if u >= 0 and v >= 0 and w >= 0:  # Point inside the triangle
                    # Color interpolation
                    color = u * c0 + v * c1 + w * c2
                    pixels[x, y] = tuple(color.astype(int))

    image.show()
    return image

def gouraud_shading_with_depth_buffer(vertices, triangles, image_size=(512, 512)):
    image = Image.new("RGB", image_size, "black")
    pixels = image.load()

    # Depth buffer 초기화
    depth_buffer = np.full(image_size, np.inf)

    light_pos = np.array([-4, 4, -3])
    light_intensity = {'ambient': 0.2, 'point': 1.0}
    material = {'ka': np.array([0, 1, 0]), 'kd': np.array([0, 0.5, 0]), 'ks': np.array([0.5, 0.5, 0.5]), 'p': 32}

    vertex_colors = {}

    # Calculate color at each vertex
    for i in range(len(vertices)):
        normal = calculate_normal_at_vertex(vertices, triangles, i)
        color = calculate_lighting(normal, light_pos, vertices[i], material, light_intensity)
        vertex_colors[i] = np.clip(color, 0, 255).astype(int)  # Ensure the color values are within RGB bounds

    for triangle in triangles:
        v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
        c0, c1, c2 = vertex_colors[triangle[0]], vertex_colors[triangle[1]], vertex_colors[triangle[2]]

        # Triangle bounding box for rasterization
        xmin = max(min(v0[0], v1[0], v2[0]), 0)
        xmax = min(max(v0[0], v1[0], v2[0]), image_size[0] - 1)
        ymin = max(min(v0[1], v1[1], v2[1]), 0)
        ymax = min(max(v0[1], v1[1], v2[1]), image_size[1] - 1)

        for x in range(int(xmin), int(xmax) + 1):
            for y in range(int(ymin), int(ymax) + 1):
                u, v, w = barycentric_coords(np.array([x, y]), v0[:2], v1[:2], v2[:2])
                if u >= 0 and v >= 0 and w >= 0:  # Point inside the triangle
                    # Depth interpolation
                    depth = u * v0[2] + v * v1[2] + w * v2[2]

                    # Depth test
                    if depth < depth_buffer[x, y]:
                        # Color interpolation
                        color = u * c0 + v * c1 + w * c2
                        pixels[x, y] = tuple(color.astype(int))

                        # Update depth buffer
                        depth_buffer[x, y] = depth

    image.show()
    return image

def calculate_normal_at_vertex(vertices, triangles, vertex_idx):
    normals = []
    for tri in triangles:
        if vertex_idx in tri:
            v0, v1, v2 = vertices[tri[0]], vertices[tri[1]], vertices[tri[2]]
            normal = calculate_triangle_normal(vertices, [tri[0], tri[1], tri[2]])
            normals.append(normal)
    vertex_normal = np.mean(normals, axis=0)
    return -vertex_normal

def calculate_lighting(normal, light_pos, view_dir, material, light_intensity):
    normal = normal / np.linalg.norm(normal)
    light_dir = light_pos - view_dir
    light_dir = light_dir / np.linalg.norm(light_dir)
    #view_dir = -view_dir  # Camera looks along -view_dir

    ambient = material['ka'] * light_intensity['ambient']
    diffuse = max(np.dot(normal, light_dir), 0) * material['kd']
    reflect_dir = 2 * np.dot(normal, light_dir) * normal - light_dir
    reflect_dir = reflect_dir / np.linalg.norm(reflect_dir)
    view_dir = view_dir / np.linalg.norm(view_dir)
    specular_strength = np.power(max(np.dot(reflect_dir, -view_dir), 0), material['p'])
    specular = specular_strength * material['ks']

    color = ambient + light_intensity['point'] * (diffuse + specular)
    #color = ambient + light_intensity['point'] * (diffuse)
    return np.clip(color, 0, 1) * 255  # Convert to RGB scale and clip


near = -0.1
far = -1000

# Create sphere data
vertices, triangles = create_scene()
transformed_vertices = transform_sphere(vertices)

# Apply perspective projection transform
transformed_vertices = perspective_projection(transformed_vertices, -0.1, 0.1, -0.1, 0.1, near, far)

# Apply viewport transform
transformed_vertices = viewport_transform(transformed_vertices, 512, 512)

#image_gouraud_shading = gouraud_shading(transformed_vertices, triangles, image_size=(512, 512))
#image_gouraud_shading.save("rasterized_sphere_with_gouraud_shading.png")


# depth buffer를 이용하여 가장 가까운 삼각형만을 그림으로 렌더링
image_gouraud_shading_with_depth_buffer = gouraud_shading_with_depth_buffer(transformed_vertices, triangles, image_size=(512, 512))
image_gouraud_shading_with_depth_buffer.save("rasterized_sphere_with_gouraud_shading_and_depth_buffer.png")
