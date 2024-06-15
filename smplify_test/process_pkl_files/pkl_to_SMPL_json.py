import os
import pickle
import json
import numpy as np
from scipy.spatial.transform import Rotation as R

def load_pkl(pkl_dir, file_path):
    with open(os.path.join(pkl_dir, file_path), 'rb') as f:
        data = pickle.load(f)
    return data

def axis_angle_to_quaternion(axis_angle):
    r = R.from_rotvec(axis_angle)
    return r.as_quat()  # Returns (x, y, z, w) format

def compute_global_rotations(pose_quaternions, bone_parents):
    global_rotations = [None] * len(bone_parents)
    for idx, parent_idx in enumerate(bone_parents):
        if parent_idx == -1:
            global_rotations[idx] = pose_quaternions[idx]
        else:
            parent_global_rotation = R.from_quat(global_rotations[parent_idx])
            local_rotation = R.from_quat(pose_quaternions[idx])
            global_rotation = parent_global_rotation * local_rotation
            global_rotations[idx] = global_rotation.as_quat().tolist()
    return global_rotations

def extract_parameters(pkl_dir, pkl_files, index_mapping, bone_parents):
    poses = []
    shapes = []
    
    #for i, pkl_file in enumerate(pkl_files):
    for pkl_file in pkl_files:
        i = int(pkl_file.split('_')[-1].split('.')[0])
        data = load_pkl(pkl_dir, pkl_file)
        cam_t = data['cam_t']
        pose = data['pose']
        betas = data['betas']
        
        pose_quaternions = []
        for j in range(0, len(pose), 3):
            axis_angle = pose[j:j+3]
            quaternion = axis_angle_to_quaternion(axis_angle)
            pose_quaternions.append(quaternion.tolist())

        # Reorder pose_quaternions according to index_mapping
        reordered_pose_quaternions = [pose_quaternions[idx] for idx in index_mapping]

        # Compute global rotations
        global_rotations = compute_global_rotations(reordered_pose_quaternions, bone_parents)
                
        # Negate the cam_t values (meter to centimeter)
        negated_cam_t = (-cam_t[0] / 1, -cam_t[1] / 1, -cam_t[2] / 1)
        negated_cam_t = (0, 0, 0)
        
        poses.append({
            "F": i,
            "R": global_rotations,
            #"T": cam_t.tolist()
            "T": negated_cam_t
        })
        shapes.append({
            "F": i,
            "S": betas.tolist()
        })

    return poses, shapes

def calculate_average_shape(shapes):
    shape_params = [shape["S"] for shape in shapes]
    shape_params_avg = np.mean(shape_params, axis=0).tolist()
    return shape_params_avg

def save_json(data, file_path):
    with open(file_path, 'w') as f:
        json.dump(data, f, indent=3)

def main():
    pkl_dir = "Seq3_Right"
    #pkl_files = sorted([os.path.join(pkl_dir, f) for f in os.listdir(pkl_dir) if f.endswith('.pkl') and not f.endswith('_2.pkl')])
    pkl_files = [f for f in os.listdir(pkl_dir) if f.endswith('.pkl') and f.count('_') != 4]
    pkl_files.sort(key=lambda x: int(x.split('_')[-1].split('.')[0]))

    # Index mapping from SMPL to GroundTruth
    boneIndexNamesSMPL = [
        "Pelvis", "L_Hip", "R_Hip", "SpineL", "L_Knee", "R_Knee", "SpineM",
        "L_Ankle", "R_Ankle", "SpineH", "L_Foot", "R_Foot", "Neck",
        "L_Collar", "R_Collar", "Head", "L_Shoulder", "R_Shoulder",
        "L_Elbow", "R_Elbow", "L_Wrist", "R_Wrist", "L_Hand", "R_Hand"
    ]
    
    boneIndexNamesGroundTruth = [
        "Pelvis", "L_Hip", "L_Knee", "L_Ankle", "L_Foot", "R_Hip", "R_Knee",
        "R_Ankle", "R_Foot", "SpineL", "SpineM", "SpineH", "Neck", "Head",
        "L_Collar", "L_Shoulder", "L_Elbow", "L_Wrist", "L_Hand", "R_Collar",
        "R_Shoulder", "R_Elbow", "R_Wrist", "R_Hand"
    ]

    index_mapping = [boneIndexNamesSMPL.index(bone) for bone in boneIndexNamesGroundTruth]
    bone_parents = [
        -1, 0, 1, 2, 3, 0, 5, 6, 7, 0, 9, 10, 11, 12, 11, 14, 15, 16, 17,
        11, 19, 20, 21, 22
    ]

    poses, shapes = extract_parameters(pkl_dir, pkl_files, index_mapping, bone_parents)
    shape_params_avg = calculate_average_shape(shapes)

    shape_data = {
        "_README": [
            ["// -----------------------------------------------"],
            ["// --- [shape_param_avg]: use this for the representative"],
            ["// --- The current shape param values in cm scale."],
            ["// --- The official SMPL uses meter scale."],
            ["// --- num_frames: the number of frames"],
            ["// --- [shape_param_frames][F]: frame index"],
            ["// --- [shape_param_frames][S]: shape parameters"],
            ["// -----------------------------------------------"]
        ],
        "gender": "NEUTRAL",
        "num_frames": len(pkl_files),  # Adjusting to the number of processed frames
        "shape_param_avg": shape_params_avg,
        "shape_param_fit": {},
        "shape_param_frames": shapes
    }

    pose_data = {
        "BoneNames": boneIndexNamesGroundTruth,
        "BoneParents": bone_parents,
        "_README": [
            ["// -----------------------------------------------"],
            ["// --- Rotations in Quaternion (x,y,z,w)"],
            ["// --- The coordinate system in OpenGL(x:left, y:up, z:forward)"],
            ["// --- Each Rotation represents Global Transform of each bone"],
            ["// --- GlobalTrans = GlobalTrans_parent * LocalTransform"],
            ["// --- LocalTransform = inv(GlobalTrans_parent) * GlobalTrans"],
            ["// --- pose_parameters: F = frame index"],
            ["// --- pose_parameters: T = translation"],
            ["// --- pose_parameters: R = rotations"],
            ["// -----------------------------------------------"]
        ],
        "num_frames": len(pkl_files),  # Adjusting to the number of processed frames
        "pose_parameters": poses
    }

    save_json(pose_data, "Frameset_SMPL_Pose.json")
    save_json(shape_data, "Frameset_SMPL_Shape.json")

if __name__ == "__main__":
    main()
