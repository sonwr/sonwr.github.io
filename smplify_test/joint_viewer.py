import json
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# Create a 3D plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Robot
joint_parent_robot = [0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8]
file_path_robot = 'robot_test/f_1100_3_joint_3d.json'

with open(file_path_robot, 'r') as file:
    joint_data_robot = json.load(file)

for idx, (x, y, z) in enumerate(joint_data_robot):
    # Plot the joint
    ax.scatter(x, y, z, color='red')

    # Add index label
    ax.text(x, y, z, 'D: %s' % (str(idx)), size=8, zorder=1)

    # Draw line to parent joint, if it exists
    if idx < len(joint_parent_robot):
        parent_idx = joint_parent_robot[idx]
        if parent_idx < len(joint_data_robot):
            parent_x, parent_y, parent_z = joint_data_robot[parent_idx]
            ax.plot([x, parent_x], [y, parent_y], [z, parent_z], 'r')



# SMPLify
joint_parent_smplify = [3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 12, 12, 24, 13, 14, 16, 17, 18, 19, 20, 21, 24]
file_path_smplify = 'output_world_jtr3.json'

with open(file_path_smplify, 'r') as file:
    joint_data_smplify = json.load(file)


# 1. z-axis flip
joint_data_smplify = [[x, y, -z] for x, y, z in joint_data_smplify]


# 2. shoulder length scale
def calculate_distance(point1, point2):
    return np.sqrt((point1[0] - point2[0])**2 + (point1[1] - point2[1])**2 + (point1[2] - point2[2])**2)

robot_distance = calculate_distance(joint_data_robot[2], joint_data_robot[1])
smplify_distance = calculate_distance(joint_data_smplify[16], joint_data_smplify[17])
scale = robot_distance / smplify_distance
joint_data_smplify = [[x * scale, y * scale, z * scale] for x, y, z in joint_data_smplify]

# 3. neck position align
robot_point = joint_data_robot[0]
smplify_point = joint_data_smplify[12]

dx, dy, dz = robot_point[0] - smplify_point[0], robot_point[1] - smplify_point[1], robot_point[2] - smplify_point[2]
joint_data_smplify = [[x + dx, y + dy, z + dz] for x, y, z in joint_data_smplify]



for idx, (_x, _y, _z) in enumerate(joint_data_smplify):

    scale = 1
    x = _x * scale
    y = _y * scale
    z = _z * scale

    # Plot the joint
    ax.scatter(x, y, z, color='blue')

    # Add index label
    ax.text(x, y, z, 'S: %s' % (str(idx)), size=8, zorder=1)

    # Draw line to parent joint, if it exists
    if idx < len(joint_parent_smplify):
        parent_idx = joint_parent_smplify[idx]
        if parent_idx < len(joint_data_smplify):
            parent_x, parent_y, parent_z = joint_data_smplify[parent_idx]
            ax.plot([x, parent_x], [y, parent_y], [z, parent_z], 'b')


# Camera position and orientation
camera_position = [0, 0, 0]
camera_orientation = np.eye(3)  # Identity matrix for rotation

# Plotting camera position
ax.scatter(*camera_position, color='black')

# Colors for the X, Y, and Z axes
axis_colors = ['red', 'green', 'blue']

# Plotting camera orientation axes (X, Y, Z) with different colors
for i, color in enumerate(axis_colors):
    direction = camera_orientation[:, i]

    # Combine camera position and direction for Python 2.7 compatibility
    combined = camera_position + list(direction)
    ax.quiver(*combined, length=50.0, color=color, arrow_length_ratio=0.3)

# Setting labels for axes
ax.set_xlabel('X axis')
ax.set_ylabel('Y axis')
ax.set_zlabel('Z axis')

ax.set_xlim([-150, 150])
ax.set_ylim([-150, 150])
ax.set_zlim([-50, 350])

# Show plot
plt.show()
