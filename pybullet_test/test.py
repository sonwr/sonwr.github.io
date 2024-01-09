import pybullet as p
import pybullet_data
import numpy as np
import time

# Initialize PyBullet and set up the environment
p.connect(p.GUI)
p.setAdditionalSearchPath(pybullet_data.getDataPath())
p.setGravity(0, 0, -10)
planeId = p.loadURDF("plane.urdf")

camera_distance = 23  # Distance from the target point
camera_yaw = 0  # Rotation angle around the vertical axis
camera_pitch = -30  # Downward angle
camera_target_position = [0, 0, 0]  # Center of the scene
p.resetDebugVisualizerCamera(camera_distance, camera_yaw, camera_pitch, camera_target_position)


# Function to create a box at a given position with a given weight
def create_box(position, weight, maxWeight):
    visual_shape_id = p.createVisualShape(shapeType=p.GEOM_BOX, rgbaColor=[1-weight/maxWeight, 1 - weight/maxWeight, 1 - weight/maxWeight, 1], halfExtents=[0.5, 0.5, 0.5])
    collision_shape_id = p.createCollisionShape(shapeType=p.GEOM_BOX, halfExtents=[0.5, 0.5, 0.5])
    return p.createMultiBody(baseMass=weight, baseCollisionShapeIndex=collision_shape_id, baseVisualShapeIndex=visual_shape_id, basePosition=position)

# Create several boxes with varying weights
for i in range(100):
    random_position = np.random.uniform(-10, 10, size=3)
    create_box(random_position, weight=i + 1, maxWeight=100)


def get_camera_position_from_view_matrix(view_matrix):
    view_matrix = np.array(view_matrix).reshape(4, 4).T
    inv_view_matrix = np.linalg.inv(view_matrix)
    camera_position = inv_view_matrix[:3, 3]
    return camera_position

# Function to shoot a sphere
def shoot_sphere(mouse_x, mouse_y, sphere_weight):
    cam_info = p.getDebugVisualizerCamera()
    cam_view_matrix = cam_info[2]
    cam_pos = get_camera_position_from_view_matrix(cam_view_matrix)
    cam_forward = cam_info[5]

    # Starting position of the sphere is just in front of the camera
    start_pos = [cam_pos[0] + cam_forward[0] * 0.5, 
                 cam_pos[1] + cam_forward[1] * 0.5, 
                 cam_pos[2] + cam_forward[2] * 0.5]

    print(cam_info[3])

    # Sphere properties
    sphere_radius = 1
    colSphereId = p.createCollisionShape(p.GEOM_SPHERE, radius=sphere_radius)
    visual_shape_id = p.createVisualShape(shapeType=p.GEOM_SPHERE, rgbaColor=[1, 0, 0, 1], radius=sphere_radius)
    sphereUid = p.createMultiBody(baseMass=sphere_weight, baseCollisionShapeIndex=colSphereId, baseVisualShapeIndex=visual_shape_id, basePosition=start_pos)

    # Shooting direction is the camera's forward direction
    shooting_direction = np.array(cam_forward)
    shooting_direction = shooting_direction / np.linalg.norm(shooting_direction)
    
    # Apply force to shoot the sphere
    force_magnitude = 5000 * sphere_weight
    p.applyExternalForce(sphereUid, -1, force_magnitude * shooting_direction, start_pos, p.WORLD_FRAME)

# Main loop
while True:
    p.stepSimulation()
    time.sleep(1./240.)

    me = p.getMouseEvents()
    
    if len(me) > 0:
        if len(me[0]) > 0:
            e = me[0]
            if e[0] == 2:
                shoot_sphere(e[1], e[2], sphere_weight=np.random.randint(1, 100))
