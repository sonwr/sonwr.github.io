
# Camera Coordinate Transformation

This repository contains a Python script for transforming 3D world coordinates to 2D pixel coordinates in an image, using the camera's intrinsic and extrinsic parameters. It demonstrates a basic but essential technique in computer vision and photogrammetry.

## Description

The script `camera_transform.py` performs the transformation of a point in the world coordinate system to the image coordinate system of a camera. This process involves the use of the camera's intrinsic parameters (focal length, principal point, etc.) and extrinsic parameters (rotation and translation matrices).

## Usage

To use the script, you need to have Python installed on your system along with the `numpy`, `opencv-python`, and `matplotlib` libraries.

1. Clone the repository.
2. Place your image in the root directory of the repository or modify the script to point to your image's path.
3. Run the script.

## Script Details

The script includes the following key components:

- **Camera Intrinsic Parameters (K):** Matrix defining the camera's internal characteristics like focal length and principal point.
- **Rotation Matrix (r):** Defines the rotation of the camera with respect to the world coordinates.
- **Translation Vector (t):** Represents the translation of the camera with respect to the world coordinates.
- **Homogeneous Coordinates Transformation:** Transforms a point from the world coordinate system to the camera coordinate system.
- **Projection onto the Image Plane:** The camera coordinates are projected onto the image plane using the intrinsic matrix.
- **Normalization and Display:** The projected point is normalized to obtain pixel coordinates, which are then marked on the image.
