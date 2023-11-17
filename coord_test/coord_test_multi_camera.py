import numpy as np
import cv2
import matplotlib.pyplot as plt


def pixel_to_world(pixel_coord, depth, K, r, t):
    # Invert the intrinsic matrix
    K_inv = np.linalg.inv(K)
    # Convert pixel to normalized camera coordinates
    camera_coord = K_inv.dot(np.array([[pixel_coord[0]], [pixel_coord[1]], [1]])) * depth
    # Invert the rotation matrix
    r_inv = np.linalg.inv(r)
    # Ensure the translation vector is in the correct shape (3, 1)
    t = t.reshape(3, 1)
    # Compute world coordinates
    world_coord = r_inv.dot(camera_coord - t)
    return world_coord[:3, 0]  # Ensure to return a 1D array of size 3

def world_to_pixel(world_coord, K, r, t):
    # Ensure the world coordinates are in the correct shape (3, 1)
    if world_coord.shape != (3, 1):
        world_coord = np.reshape(world_coord, (3, 1))
    # Ensure the translation vector is in the correct shape (3, 1)
    t = t.reshape(3, 1)
    # Compute camera coordinates
    camera_coord = r.dot(world_coord) + t
    # Project camera coordinates to pixel coordinates using intrinsic matrix K
    pixel_coord_homogeneous = K.dot(camera_coord)
    # Normalize homogeneous coordinates to get pixel coordinates
    pixel_coord = pixel_coord_homogeneous / pixel_coord_homogeneous[2]
    return int(pixel_coord[0, 0]), int(pixel_coord[1, 0])



# Function to compute pixel coordinates from world coordinates
def compute_pixel_coordinates(K, r, t, lens_distortion, world_point):
    # Form the extrinsic matrix from r and t
    extrinsic_matrix = np.hstack((r, t.T))
    # Compute the camera coordinates
    camera_coord = extrinsic_matrix * world_point.reshape(-1, 1)
    # Correct for lens distortion
    undistorted_camera_coord = compute_lens_distortion(camera_coord, lens_distortion)
    # Project the undistorted camera coordinates into pixel coordinates
    pixel_coords = K * undistorted_camera_coord[:3]
    # Convert from homogeneous coordinates
    pixel_coords = pixel_coords / pixel_coords[2]
    return pixel_coords

def compute_lens_distortion(camera_coord, lens_distortion):
    d1, d2, t1, t2, d3, d4, d5, d6 = lens_distortion
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

# Define the camera parameters for Camera 1
K1 = np.matrix([
    [726.012573, 0.000000, 615.539917],
    [0.000000, 724.575256, 523.566040],
    [0.000000, 0.000000, 1.000000],
])

r1 = np.matrix([
    [-0.262846, -0.012075, 0.964762],
    [-0.006363, -0.999878, -0.014248],
    [0.964817, -0.009883, 0.262737],
])

t1 = np.matrix([-3.506606, 128.004700, 168.618835])

distortion1 = np.array([-0.361906, 0.179266, -0.000213, 0.001141, -0.054861, 0, 0, 0])



# Define the camera parameters for Camera 2
K2 = np.matrix([
    [729.434204, 0.000000, 605.735596],
    [0.000000, 728.705566, 511.485962],
    [0.000000, 0.000000, 1.000000],
])

r2 = np.matrix([
    [0.108824, 0.005068, 0.994048],
    [-0.052556, -0.998559, 0.010845],
    [0.992671, -0.053423, -0.108402],
])

t2 = np.matrix([-0.255262, 129.808014, 177.681732])

distortion2 = np.array([-0.366454, 0.227449, 0.000769, -0.000390, -0.127463, 0, 0, 0])


# Homogeneous coordinate for world point (0,0,0)
p = np.array([0, 0, 0, 1])  

# Compute pixel coordinates for both cameras
pixel_coords1 = compute_pixel_coordinates(K1, r1, t1, distortion1, p)
pixel_coords2 = compute_pixel_coordinates(K2, r2, t2, distortion2, p)




# Function to plot image and point
def plot_image_with_point(image_path, pixel_coords, camera_title, figure):
    # Load the image using OpenCV
    image = cv2.imread(image_path)
    # Convert color from BGR to RGB for plotting
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    # Plot the image
    plt.figure(figure)
    plt.title(camera_title)
    plt.imshow(image)    
    plt.scatter([pixel_coords[0]], [pixel_coords[1]], c='blue', marker='x')
    
# Function to plot image and point
def plot_image_with_point_xy(image_path, pixel_coords_x, pixel_coords_y, camera_title, figure):
    # Load the image using OpenCV
    image = cv2.imread(image_path)
    # Convert color from BGR to RGB for plotting
    image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    # Plot the image
    plt.figure(figure)
    plt.title(camera_title)
    plt.imshow(image)    
    plt.scatter(pixel_coords_x, pixel_coords_y, c='blue', marker='x')
    

# Plot for Camera 1 - World to Pixel Coordinate System
plot_image_with_point('Hik_2_frame2189.jpg', pixel_coords1, 'Camera 1', 'Figure 1')

# Plot for Camera 2 - World to Pixel Coordinate System
plot_image_with_point('Hik_3_frame2189.jpg', pixel_coords2, 'Camera 2', 'Figure 2')




# Plot - Pixel Coord for Camera 1 to Pixel Coord for Camera 2
# Pixel Coord (Camera 1) -> World Coord -> Pixel Coord (Camera 2)
x_array1 = np.empty(0, int)
y_array1 = np.empty(0, int)

x_array2 = np.empty(0, int)
y_array2 = np.empty(0, int)

for i in range(1, 15):
    j = i * 50

    x = j
    y = 200
    d = 150

    world_coord_from_camera1_p1 = pixel_to_world((x, y), d, K1, r1, t1)
    camera2_pixel_from_world_coord_p1 = world_to_pixel(world_coord_from_camera1_p1, K2, r2, t2)

    x_array1 = np.append(x_array1, x)
    y_array1 = np.append(y_array1, y)
    
    x_array2 = np.append(x_array2, camera2_pixel_from_world_coord_p1[0])
    y_array2 = np.append(y_array2, camera2_pixel_from_world_coord_p1[1])


# Plot for Camera 1
array1 = [(100, 100), (150, 150), (200, 200)]
plot_image_with_point_xy('Hik_2_frame2189.jpg', x_array1, y_array1, 'Camera 1', 'Figure 3')

# Plot for Camera 2
plot_image_with_point_xy('Hik_3_frame2189.jpg', x_array2, y_array2, 'Camera 2', 'Figure 4')


plt.show()