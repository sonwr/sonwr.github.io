import os
import pickle
import json
import numpy as np
from smpl_webuser.serialization import load_model
from smpl_webuser.verts import verts_decorated
from smpl_webuser.lbs import global_rigid_transformation
import chumpy as ch
import cv2

smpl_to_openpose_map = {
    "Pelvis": "MidHip",
    "L_Hip": "L_Hip",
    "R_Hip": "R_Hip",
    "SpineL": None, # SpineL doesn't map directly to Openpose
    "L_Knee": "L_Knee",
    "R_Knee": "R_Knee",
    "SpineM": None, # SpineM doesn't map directly to Openpose
    "L_Ankle": "L_Ankle",
    "R_Ankle": "R_Ankle",
    "SpineH": None, # SpineH doesn't map directly to Openpose
    "L_Foot": None, # L_Foot doesn't map directly to Openpose
    "R_Foot": None, # R_Foot doesn't map directly to Openpose
    "Neck": "Neck",
    "L_Collar": None, # L_Collar doesn't map directly to Openpose
    "R_Collar": None, # R_Collar doesn't map directly to Openpose
    "Head": None, # Head doesn't map directly to Openpose
    "L_Shoulder": "L_Shoulder",
    "R_Shoulder": "R_Shoulder",
    "L_Elbow": "L_Elbow",
    "R_Elbow": "R_Elbow",
    "L_Wrist": "L_Wrist",
    "R_Wrist": "R_Wrist",
    "L_Hand": None, # L_Hand doesn't map directly to Openpose
    "R_Hand": None  # R_Hand doesn't map directly to Openpose
}

def load_pkl_file(file_path):
    with open(file_path, 'rb') as f:
        params = pickle.load(f)
    return params

def extract_joint_positions(model, pose, betas, cam_t):
    # Number of shape coefficients
    n_betas = len(betas)

    # Create SMPL model instance
    sv = verts_decorated(
        trans=ch.array(cam_t),
        pose=ch.array(pose),
        v_template=model.v_template,
        J=model.J_regressor,
        betas=ch.array(betas),
        shapedirs=model.shapedirs[:, :, :n_betas],
        weights=model.weights,
        kintree_table=model.kintree_table,
        bs_style=model.bs_style,
        f=model.f,
        bs_type=model.bs_type,
        posedirs=model.posedirs)

    # Compute the 3D joint positions
    Jdirs = np.dstack([model.J_regressor.dot(model.shapedirs[:, :, i]) for i in range(n_betas)])
    J_onbetas = ch.array(Jdirs).dot(ch.array(betas)) + model.J_regressor.dot(model.v_template.r)
    _, A_global = global_rigid_transformation(ch.array(pose), J_onbetas, model.kintree_table, xp=ch)
    joints_3d = np.vstack([g[:3, 3] for g in A_global])

    return joints_3d





def reorder_cam_t(cam_t, smpl_to_openpose_map):
    # For cam_t, the mapping will be similar to joints since it represents translations
    ordered_cam_t = [0.0, 0.0, 0.0]
    # Map the cam_t translation values based on the mapping
    for smpl_joint, openpose_joint in smpl_to_openpose_map.items():
        if openpose_joint:
            ordered_cam_t = cam_t
    return ordered_cam_t

def reorder_quaternions(quaternions, smpl_to_openpose_map):
    # Create a list of default quaternion [0, 0, 0, 1] for OpenPose order
    ordered_quaternions = [[0.0, 0.0, 0.0, 1.0] for _ in range(25)]
    smpl_order = [
        "Pelvis", "L_Hip", "R_Hip", "SpineL", "L_Knee", "R_Knee",
        "SpineM", "L_Ankle", "R_Ankle", "SpineH", "L_Foot", "R_Foot",
        "Neck", "L_Collar", "R_Collar", "Head", "L_Shoulder",
        "R_Shoulder", "L_Elbow", "R_Elbow", "L_Wrist", "R_Wrist",
        "L_Hand", "R_Hand"
    ]

    # Create a dictionary for easy access
    quaternion_dict = {smpl_order[i]: quaternions[i] for i in range(len(smpl_order))}
    
    for openpose_index, openpose_name in enumerate(smpl_to_openpose_map.keys()):
        smpl_name = smpl_to_openpose_map.get(openpose_name)
        if smpl_name and smpl_name in quaternion_dict:
            ordered_quaternions[openpose_index] = quaternion_dict[smpl_name]
    
    return ordered_quaternions

def reorder_betas(betas, smpl_to_openpose_map):
    # Typically, betas are not reordered as they represent shape coefficients
    # This function is added for the completeness of the request
    ordered_betas = list(betas)  # Assuming betas don't need reordering
    return ordered_betas




def reorder_joints_deeprobot_to_openpose(joints):
    # The mapping from DeepRobotJoint to Openpose order
    reorder_map = {
        0: 1,  # Neck -> Neck
        1: 5,  # L_Shoulder -> L_Shoulder
        2: 2,  # R_Shoulder -> R_Shoulder
        3: 12, # L_Hip -> L_Hip
        4: 9,  # R_Hip -> R_Hip
        5: 6,  # L_Elbow -> L_Elbow
        6: 3,  # R_Elbow -> R_Elbow
        7: 13, # L_Knee -> L_Knee
        8: 10, # R_Knee -> R_Knee
        9: 7,  # L_Wrist -> L_Wrist
        10: 4, # R_Wrist -> R_Wrist
        11: 14, # L_Ankle -> L_Ankle
        12: 11  # R_Ankle -> R_Ankle
    }
    
    ordered_joints = [[0.0, 0.0, 0.0] for _ in range(25)]
    for deep_index, openpose_index in reorder_map.items():
        ordered_joints[openpose_index] = joints[deep_index]
    
    return ordered_joints

def reorder_joints(joints):
    # The mapping from SMPL to Openpose order
    
    openpose_order = [
        "Nose", "Neck", "R_Shoulder", "R_Elbow", "R_Wrist", 
        "L_Shoulder", "L_Elbow", "L_Wrist", "MidHip", "R_Hip", 
        "R_Knee", "R_Ankle", "L_Hip", "L_Knee", "L_Ankle", 
        "REye", "LEye", "REar", "LEar", "LBigToe", "LSmallToe", 
        "LHeel", "RBigToe", "RSmallToe", "RHeel"
    ]
    
    # Default value for joints that don't exist in the original data
    default_joint = [0.0, 0.0, 0.0]
    smpl_order = [
        "Pelvis", "L_Hip", "R_Hip", "SpineL", "L_Knee", "R_Knee", 
        "SpineM", "L_Ankle", "R_Ankle", "SpineH", "L_Foot", "R_Foot", 
        "Neck", "L_Collar", "R_Collar", "Head", "L_Shoulder", 
        "R_Shoulder", "L_Elbow", "R_Elbow", "L_Wrist", "R_Wrist", 
        "L_Hand", "R_Hand"
    ]
    
    # Create a dictionary for easy access
    joints_dict = {smpl_order[i]: joints[i] for i in range(len(smpl_order))}
    
    ordered_joints = [default_joint] * 25
    for openpose_index, openpose_name in enumerate(openpose_order):
        smpl_name = smpl_to_openpose_map.get(openpose_name)
        if smpl_name:
            ordered_joints[openpose_index] = joints_dict[smpl_name]
    
    return ordered_joints

def extract_joint_rotations(model, pose, betas):
    # Number of shape coefficients
    n_betas = len(betas)

    # Create SMPL model instance
    sv = verts_decorated(
        trans=ch.zeros(3),
        pose=ch.array(pose),
        v_template=model.v_template,
        J=model.J_regressor,
        betas=ch.array(betas),
        shapedirs=model.shapedirs[:, :, :n_betas],
        weights=model.weights,
        kintree_table=model.kintree_table,
        bs_style=model.bs_style,
        f=model.f,
        bs_type=model.bs_type,
        posedirs=model.posedirs)

    # Compute the 3D joint rotations
    Jdirs = np.dstack([model.J_regressor.dot(model.shapedirs[:, :, i]) for i in range(n_betas)])
    J_onbetas = ch.array(Jdirs).dot(ch.array(betas)) + model.J_regressor.dot(model.v_template.r)
    _, A_global = global_rigid_transformation(ch.array(pose), J_onbetas, model.kintree_table, xp=ch)
    rotations = [g[:3, :3] for g in A_global]

    return rotations

def quaternion_from_matrix(matrix):
    R = matrix[:3, :3]
    q = np.empty((4, ))
    t = np.trace(R)
    if t > 0.0:
        t = np.sqrt(t + 1.0)
        q[0] = t * 0.5
        t = 0.5 / t
        q[1] = (R[2, 1] - R[1, 2]) * t
        q[2] = (R[0, 2] - R[2, 0]) * t
        q[3] = (R[1, 0] - R[0, 1]) * t
    else:
        i = 0
        if R[1, 1] > R[0, 0]:
            i = 1
        if R[2, 2] > R[i, i]:
            i = 2
        j = (i + 1) % 3
        k = (j + 1) % 3
        t = np.sqrt(R[i, i] - R[j, j] - R[k, k] + 1.0)
        q[i + 1] = t * 0.5
        t = 0.5 / t
        q[0] = (R[k, j] - R[j, k]) * t
        q[j + 1] = (R[j, i] + R[i, j]) * t
        q[k + 1] = (R[k, i] + R[i, k]) * t
    return q

def create_json_files(input_dir, output_dir, model_path):
    # Load SMPL model
    model = load_model(model_path)

    # Prepare JSON structures
    pos_data = {
        "Header": {
            "Comment": [
                "Hip: center of L_hip, R_hip",
                "Only the first 15 joints are used among opose 25 joints",
                "Set-F : Frame Index",
                "Set-J : Joint Pos3D (x, y, z, confidence) in Opose25 type"
            ],
            "JOINT_NAMES": [
                "Nose", "Neck", "RShoulder", "RElbow", "RWrist",
                "LShoulder", "LElbow", "LWrist", "MidHip", "RHip",
                "RKnee", "RAnkle", "LHip", "LKnee", "LAnkle", "REye",
                "LEye", "REar", "LEar", "LBigToe", "LSmallToe", "LHeel",
                "RBigToe", "RSmallToe", "RHeel"
            ],
            "NUM_FRAMES": 1378,
            "NUM_JOINTS": 25,
            "PARENT_IDS": [
                1, 8, 1, 2, 3, 1, 5, 6, -1, 8, 9, 10, 8, 12, 13,
                0, 0, 15, 16, 14, 19, 14, 11, 22, 11
            ]
        },
        "Set": []
    }

    pose_data = {
        "BoneNames": [
            "Pelvis", "L_Hip", "L_Knee", "L_Ankle", "L_Foot", "R_Hip",
            "R_Knee", "R_Ankle", "R_Foot", "SpineL", "SpineM", "SpineH",
            "Neck", "Head", "L_Collar", "L_Shoulder", "L_Elbow",
            "L_Wrist", "L_Hand", "R_Collar", "R_Shoulder", "R_Elbow",
            "R_Wrist", "R_Hand"
        ],
        "BoneParents": [
            -1, 0, 1, 2, 3, 0, 5, 6, 7, 0, 9, 10, 11, 12, 11, 14, 15,
            16, 17, 11, 19, 20, 21, 22
        ],
        "_README": [
            [ "// -----------------------------------------------" ],
            [ "// --- Rotations in Quaternion (x,y,z,w)" ],
            [ "// --- The coordinate system in OpenGL(x:left, y:up, z:forward)" ],
            [ "// --- Each Rotation represents Global Transform of each bone" ],
            [ "// --- GlobalTrans = GlobalTrans_parent * LocalTransform" ],
            [ "// --- LocalTransform = inv(GlobalTrans_parent) * GlobalTrans" ],
            [ "// --- pose_parameters: F = frame index" ],
            [ "// --- pose_parameters: T = translation" ],
            [ "// --- pose_parameters: R = rotations" ],
            [ "// -----------------------------------------------" ]
        ],
        "num_frames": 1378,
        "pose_parameters": []
    }

    shape_data = {
        "_README": [
            [ "// -----------------------------------------------" ],
            [ "// --- [shape_param_avg]: use this for the representative" ],
            [ "// --- The current shape param values in cm scale." ],
            [ "// --- The official SMPL uses meter scale." ],
            [ "// --- num_frames: the number of frames" ],
            [ "// --- [shape_param_frames][F]: frame index" ],
            [ "// --- [shape_param_frames][S]: shape parameters" ],
            [ "// -----------------------------------------------" ]
        ],
        "gender": "NEUTRAL",
        "num_frames": 1378,
        "shape_param_avg": [],
        "shape_param_fit": {
            "error_pose": 0.0,
            "error_shape": 0.0,
            "iter": 0,
            "value": []
        },
        "shape_param_frames": []
    }

    # Process each pkl file in the directory

    for frame_index in range(1370): # last frame
        file_name = "USB_Sync_Left_{}.pkl".format(frame_index)
        pkl_path = os.path.join(input_dir, file_name)    
        params = load_pkl_file(pkl_path)

        cam_t = np.array(params['cam_t'])
        pose = np.array(params['pose'])
        betas = np.array(params['betas'])

        joints_3d = extract_joint_positions(model, pose, betas, cam_t)
        ordered_joints_3d = reorder_joints(joints_3d)

        #rotations = extract_joint_rotations(model, pose, betas)
        #quaternions = [quaternion_from_matrix(rot) for rot in rotations]

        # TODO
        #ordered_cam_t, ordered_quaternions, ordered_betas = reorder_parameters(cam_t, quaternions, betas, smpl_to_openpose_map)        
        #ordered_cam_t = reorder_cam_t(cam_t, smpl_to_openpose_map)
        #ordered_quaternions = reorder_quaternions(quaternions, smpl_to_openpose_map)
        #ordered_betas = reorder_betas(betas, smpl_to_openpose_map)

        pos_entry = {
            "F": frame_index,
            "J": [list(joint) + [1.0] for joint in ordered_joints_3d]  # Assuming confidence is 1.0 for all joints
        }
        pos_data["Set"].append(pos_entry)

        #pose_entry = {
        #    "F": frame_index,
        #    "T": list(ordered_cam_t),
        #    "R": [list(q) for q in ordered_quaternions]
        #}
        #pose_data["pose_parameters"].append(pose_entry)

        #shape_entry = {
        #    "F": frame_index,
        #    "S": list(ordered_betas)
        #}
        #shape_data["shape_param_frames"].append(shape_entry)

        
        
        pose = params['pose'].tolist()
        betas = params['betas'].tolist()

        params = {
            'pose': pose,
            'betas': betas
        }

        json_path = os.path.join(output_dir, 'param_smplify_{}.json'.format(frame_index))
        with open(json_path, 'w') as f:
            json.dump(params, f, indent=4)

    # Calculate average shape parameters
    #all_shapes = np.array([frame["S"] for frame in shape_data["shape_param_frames"]])
    #shape_data["shape_param_avg"] = list(np.mean(all_shapes, axis=0))

    # Save JSON files
    with open(os.path.join(output_dir, 'pos_smplify.json'), 'w') as f:
        json.dump(pos_data, f, indent=4)

    #with open(os.path.join(output_dir, 'pose_smplify.json'), 'w') as f:
    #    json.dump(pose_data, f, indent=4)

    #with open(os.path.join(output_dir, 'shape_smplify.json'), 'w') as f:
    #    json.dump(shape_data, f, indent=4)


# Specify the input and output directories
input_dir = 'Seq1'
output_dir = 'output'
#model_path = 'path_to_smpl_model.pkl'  # Replace with the path to your SMPL model
model_path = 'models/basicModel_neutral_lbs_10_207_0_v1.0.0.pkl'  # replace with the path to your SMPL model

# Create output directory if it doesn't exist
if not os.path.exists(output_dir):
    os.makedirs(output_dir)

# Generate the JSON files
create_json_files(input_dir, output_dir, model_path)

