
# Assignment 1-3: Ray Tracing

This repository contains the implementation of a simple ray tracer designed for the Computer Graphics and Image Processing course in Spring 2024. The ray tracer is developed in Python and consists of three main parts: ray-object intersection, shading with the Phong model, and antialiasing.

## Getting Started

### Prerequisites

To run the scripts, you need Python 3 installed on your system (the code was tested with Python 3.9.5). Additionally, you'll need the NumPy and Pillow libraries. If you don't have them installed, you can install them using pip:

```bash
pip install numpy pillow
```

### Structure

The implementation is divided into three separate scripts, each corresponding to a part of the assignment:

- `lab1_ray_intersection.py`: Handles the basic ray-object intersections.
- `lab1_shading.py`: Implements the Phong shading model to add realistic lighting and shadows.
- `lab1_antialiasing.py`: Applies antialiasing to reduce jagged edges in the rendered scene.

### Running the Scripts

1. **Ray-Object Intersection**

To run the ray-object intersection part, execute:

```bash
python lab1_ray_intersection.py
```

This will generate an image named `ray_tracing_result.png` showing the intersections between rays and the scene objects.
![ray_tracing_result.png](https://github.com/sonwr/sonwr.github.io/blob/main/2024_cg/ray_tracing_result.png?raw=true)

2. **Shading**

To apply the Phong shading model, run:

```bash
python lab1_shading.py
```

This script produces `ray_tracing_phong_shading.png`, which includes shading and shadows for a more realistic appearance.
![ray_tracing_phong_shading.png](https://github.com/sonwr/sonwr.github.io/blob/main/2024_cg/ray_tracing_phong_shading.png?raw=true)

3. **Antialiasing**

To reduce aliasing artifacts with antialiasing, use:

```bash
python lab1_antialiasing.py
```

The output, `ray_tracing_antialiasing.png`, will show the final scene with significantly reduced jaggies.
![ray_tracing_antialiasing.png](https://github.com/sonwr/sonwr.github.io/blob/main/2024_cg/ray_tracing_antialiasing.png?raw=true)

## Viewing the Results

The generated images will be saved in the current directory. You can view them using any standard image viewer.

## Implementation Details

Each script utilizes ray tracing principles to render a simple scene consisting of a plane and three spheres. The process demonstrates fundamental techniques in computer graphics, including geometric intersection tests, lighting models, and antialiasing methods.

- **Ray Intersection**: Determines where rays cast from a virtual camera intersect scene objects.
- **Shading**: Applies ambient, diffuse, and specular lighting calculations based on the Phong reflection model.
- **Antialiasing**: Uses multiple samples per pixel and averages them to smooth the edges of objects.

## License

This project is licensed under the MIT License. See the LICENSE file in the repository for more information.
