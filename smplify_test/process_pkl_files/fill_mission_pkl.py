import os
import shutil

def fill_missing_files(folder_path):
    for i in range(1, 1470):
        expected_file = "USB_Sync_Right_{}.pkl".format(i)
        previous_file = "USB_Sync_Right_{}.pkl".format(i-1)
        
        expected_file_path = os.path.join(folder_path, expected_file)
        previous_file_path = os.path.join(folder_path, previous_file)
        
        if not os.path.exists(expected_file_path):
            if os.path.exists(previous_file_path):
                shutil.copy(previous_file_path, expected_file_path)
                print("copy: {} -> {}".format(previous_file, expected_file))
            else:
                print("error: {} file missing.".format(previous_file))

folder_path = "Seq3_Right"

fill_missing_files(folder_path)
