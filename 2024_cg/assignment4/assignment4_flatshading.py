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


def viewport_transform2(vertices, nx, ny, near, far):
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


def calculate_triangle_normal(vertices, triangle):
    v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
    edge1 = v1 - v0
    edge2 = v2 - v0
    normal = np.cross(edge1, edge2)

    #print("[idx] x y z: ", triangle[0], triangle[1], triangle[2])
    #print("[val] x y z: ", vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]])

    return -(normal / np.linalg.norm(normal))   # TODO: normal 반대?

def calculate_camera_direction(vertices):
    # 카메라 방향은 z 축을 따라 향하는 단위 벡터로 가정합니다.
    camera_direction = np.array([0, 0, -1])
    return camera_direction

def rasterize_triangles_with_dot_product(vertices, triangles, image_size=(512, 512)):
    image = Image.new("RGB", image_size, "black")
    pixels = image.load()
    z_buffer = np.full(image_size, np.inf)
    
    camera_direction = calculate_camera_direction(vertices)
    
    # 최대값과 최소값 초기화
    max_dot_product = -np.inf
    min_dot_product = np.inf

    max_z = -np.inf
    min_z = np.inf

    max_x = -np.inf
    min_x = np.inf

    max_y = -np.inf
    min_y = np.inf
    
    for triangle in triangles:
        v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
        normal = calculate_triangle_normal(vertices, triangle)
        
        # 카메라 방향과 normal 벡터의 dot product 계산
        dot_product = np.dot(normal, camera_direction)
        if dot_product <= 0:
            continue

        #print("dot_product:", dot_product)

        # 최대값 및 최소값 업데이트
        max_dot_product = max(max_dot_product, dot_product)
        min_dot_product = min(min_dot_product, dot_product)

        # 각 정점의 좌표값 업데이트
        for vertex in [v0, v1, v2]:
            max_x = max(max_x, vertex[0])
            min_x = min(min_x, vertex[0])
            max_y = max(max_y, vertex[1])
            min_y = min(min_y, vertex[1])
            
        # z 값의 최대값 및 최소값 업데이트
        triangle_z_values = [v0[2], v1[2], v2[2]]
        max_z = max(max_z, *triangle_z_values)
        min_z = min(min_z, *triangle_z_values)

    # 최대값과 최소값을 이용하여 정규화
    dot_product_range = max_dot_product - min_dot_product


    print("max_dot_product, min_dot_product, dot_product_range: ", max_dot_product, min_dot_product, dot_product_range)
    print("max_z, min_z: ", max_z, min_z)
    print("max_x, min_x: ", max_x, min_x)
    print("max_y, min_y: ", max_y, min_y)
    print("ratio: ", (max_x - min_x), (max_z - min_z), (max_x - min_x) / (max_z - min_z))
    
    
    for triangle in triangles:
        v0, v1, v2 = vertices[triangle[0]], vertices[triangle[1]], vertices[triangle[2]]
        normal = calculate_triangle_normal(vertices, triangle)
        
        # 카메라 방향과 normal 벡터의 dot product 계산
        dot_product = np.dot(normal, camera_direction)
        
        # 내적이 음수인 경우에만 렌더링하고, 그렇지 않으면 스킵합니다.
        if dot_product <= 0:
            continue
        
        # dot product를 [0, 1] 범위로 정규화하여 색상값 계산
        normalized_dot_product = (dot_product - min_dot_product) / dot_product_range
        #print(normalized_dot_product)
        color_value = int(normalized_dot_product * 255)
        color = (color_value, color_value, color_value)
        
        # Triangle bounding box
        xmin = max(min(v0[0], v1[0], v2[0]), 0)
        xmax = min(max(v0[0], v1[0], v2[0]), image_size[0] - 1)
        ymin = max(min(v0[1], v1[1], v2[1]), 0)
        ymax = min(max(v0[1], v1[1], v2[1]), image_size[1] - 1)
        
        for x in range(int(xmin), int(xmax) + 1):
            for y in range(int(ymin), int(ymax) + 1):
                u, v, w = barycentric_coords(np.array([x, y]), v0[:2], v1[:2], v2[:2])
                if u >= 0 and v >= 0 and w >= 0:  # Point inside the triangle
                    pixels[x, y] = color
                    
                    z = u * v0[2] + v * v1[2] + w * v2[2]
                    if z_buffer[y, x] > z:
                        z_buffer[y, x] = z
                        
    image.show()
    return image


# Create sphere data
vertices, triangles = create_scene()
transformed_vertices = transform_sphere(vertices)
#transformed_vertices = vertices

near = -0.1
far = -1000

# Apply perspective projection transform
transformed_vertices = perspective_projection(transformed_vertices, -0.1, 0.1, -0.1, 0.1, near, far)

# Apply viewport transform
#transformed_vertices = viewport_transform(transformed_vertices, 512, 512, near, far)
transformed_vertices = viewport_transform(transformed_vertices, 512, 512)

# Rasterize triangles
image_with_normals = rasterize_triangles_with_dot_product(transformed_vertices, triangles, image_size=(512, 512))
image_with_normals.save("rasterized_sphere_with_normals.png")  # Save the image file
