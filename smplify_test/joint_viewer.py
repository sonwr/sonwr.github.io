import json
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# SMPLify
joint_parent = [3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 12, 12, 24, 13, 14, 16, 17, 18, 19, 20, 21, 24]

# Robot
#joint_parent = [0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8]

# Load the JSON file
#file_path = 'robot_test/joint_3d_smplify.json'
file_path = 'robot_test/joint_3d_smplify_world.json'
#file_path = 'robot_test/f_1340_3_joint_3d_2.json'
with open(file_path, 'r') as file:
    joint_data = json.load(file)

# Create a 3D plot
fig = plt.figure()
ax = fig.add_subplot(111, projection='3d')

# Plot each joint
for idx, (x, y, z) in enumerate(joint_data):
    # Plot the joint
    ax.scatter(x, y, z)

    # Add index label
    ax.text(x, y, z, '%s' % (str(idx)), size=8, zorder=1)

    # Draw line to parent joint, if it exists
    if idx < len(joint_parent):
        parent_idx = joint_parent[idx]
        if parent_idx < len(joint_data):
            parent_x, parent_y, parent_z = joint_data[parent_idx]
            ax.plot([x, parent_x], [y, parent_y], [z, parent_z], 'r')


# Setting labels for axes
ax.set_xlabel('X axis')
ax.set_ylabel('Y axis')
ax.set_zlabel('Z axis')

# Show plot
plt.show()
