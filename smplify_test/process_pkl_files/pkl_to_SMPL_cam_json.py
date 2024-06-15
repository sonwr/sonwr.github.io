import os
import json
import pickle

def extract_camera_params(pkl_dir):
    camera_params = []
    
    # List the pkl files
    pkl_files = [f for f in os.listdir(pkl_dir) if f.endswith('.pkl') and f.count('_') != 4]
    
    # Extract frame indices and sort by them
    pkl_files.sort(key=lambda x: int(x.split('_')[-1].split('.')[0]))
    #print("Sorted PKL Files:")
    #for file in pkl_files:
    #    print(file)
    
    for pkl_file in pkl_files:
        frame_index = int(pkl_file.split('_')[-1].split('.')[0])

        with open(os.path.join(pkl_dir, pkl_file), 'rb') as f:
            params = pickle.load(f)
        
        cam_params = {
            "F": frame_index,
            "cam_t_r": params['cam_t'].tolist(),
            "cam_f_r": params['f'].tolist()
        }

        camera_params.append(cam_params)
    
    return camera_params

def save_to_json(data, output_file):
    with open(output_file, 'w') as f:
        json.dump(data, f, indent=4)

# Directory containing the pkl files
pkl_dir = 'Seq2_Right'

# Extract the camera parameters
camera_params = extract_camera_params(pkl_dir)

# Save to JSON file
output_file = 'smplify_cam.json'
save_to_json(camera_params, output_file)
