
import cv2
import numpy as np
import matplotlib.pyplot as plt

# Camera Intrinsic Parameter
K = np.matrix([
    [726.012573, 0.000000, 615.539917],
    [0.000000, 724.575256, 523.566040],
    [0.000000, 0.000000, 1.000000],
])

# Camera Extrinsic Parameter (Rotation)
r = np.matrix([
    [-0.262846, -0.012075, 0.964762],
    [-0.006363, -0.999878, -0.014248],
    [0.964817, -0.009883, 0.262737],
])

# Camera Extrinsic Parameter (Translation)
t = np.matrix([-3.506606, 128.004700, 168.618835]).T

# Camera Distortion Parameter
distortion = np.array([-0.361906, 0.179266, -0.000213, 0.001141, -0.054861, 0, 0, 0])



def lensDistortion(camera_coord):
    d1, d2, t1, t2, d3, d4, d5, d6 = distortion
    z = camera_coord[2, 0]
    x = (camera_coord[0, 0] / z) if z != 0 else 0
    x2 = x**2
    y = (camera_coord[1, 0] / z) if z != 0 else 0
    y2 = y**2
    r2 = x**2 + y**2
    r4 = r2**2
    r6 = r2**3
    r_dist = (1 + d1*r2 + d2*r4 + d3*r6) / (1 + d4*r2 + d5*r4 + d6*r6);
    if r_dist < 0.5:
        r_dist = 0.5;
        camera_coord[3, 0] = 0
    
    xprime = x*r_dist + 2*t1*x*y + t2*(r2 + 2*x2)
    yprime = y*r_dist + 2*t2*x*y + t1*(r2 + 2*y2)
    camera_coord[0, 0] = xprime * z
    camera_coord[1, 0] = yprime * z
    return camera_coord;


# 1. OpenCV (projectPoints)
# World Coordinate System (0,0,0) to 3x1 Vector
# world_point = np.array([[0], [0], [0]], dtype=np.float64)

# World Coordinate System (3D) to Pixel Coordinate System (2D)
# pixel_coords_distorted = cv2.projectPoints(world_point, r, t, K, distortion)[0].squeeze()


# 2. OpenCV (projectPoints)
# World Coordinate System (0,0,0) to 3x1 Vector
# world_point = np.array([0, 0, 0], dtype=np.float64)

# The projectPoints function expects input points in the shape of (1, N, 3).
# pixel_coords_distorted = cv2.projectPoints(world_point.reshape(1, 1, 3), r, t, K, distortion)[0].squeeze()


# 3. without OpenCV

# World Coordinate System (0,0,0) to 3x1 Vector
world_point_homogeneous = np.array([[0], [0], [0], [1]])

# World Coordinate System (3D) to Camera Coordinate System (3D)
camera_coords = (r * world_point_homogeneous[:3]) + t

# Camera Coordinate System (3D) to Pixel Coordinate System (2D)
image_point_no_distortion = K * camera_coords

# Normalize to obtain pixel coordinates
x_no_distortion = image_point_no_distortion[0, 0] / image_point_no_distortion[2, 0]
y_no_distortion = image_point_no_distortion[1, 0] / image_point_no_distortion[2, 0]
pixel_coords_no_distortion = np.array([x_no_distortion, y_no_distortion])

# Camera Coordinate System (3D) to Pixel Coordinate System (2D)
image_point = K * lensDistortion(camera_coords)

# Normalize to obtain pixel coordinates
x = image_point[0, 0] / image_point[2, 0]
y = image_point[1, 0] / image_point[2, 0]
pixel_coords_distorted = np.array([x, y])



# Load Image
image_path = 'Hik_2_frame2189.jpg'
image = cv2.imread(image_path)

# BGR to RGB
image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)

# Draw Pixel
plt.imshow(image_rgb)
plt.scatter([pixel_coords_distorted[0]], [pixel_coords_distorted[1]], c='blue', marker='x')


# Show Result Image
plt.show()

