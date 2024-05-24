
# Running SMPL, SMPLify on Windows with WSL

This guide provides detailed instructions for running SMPL on a Windows system using WSL (Windows Subsystem for Linux).

## Requirements

- Python Version: 2.7.18

## Installation Steps

### 1. Code Download
- [SMPL](https://smpl.is.tue.mpg.de/)
- [SMPLify](https://smplify.is.tue.mpg.de/)

### 2. Install Required Packages
```bash
pip install numpy scipy chumpy
pip2 install Cython==3.0.6
```

### 3. Install OpenDR
Install necessary build tools and dependencies:
```bash
sudo apt-get install build-essential
sudo apt-get install -y make build-essential libssl-dev zlib1g-dev libbz2-dev \
libreadline-dev libsqlite3-dev wget curl llvm libncurses5-dev libncursesw5-dev \
xz-utils tk-dev libffi-dev liblzma-dev
sudo apt install mesa-utils libglu1-mesa-dev freeglut3-dev mesa-common-dev
```

Clone the OpenDR repository:
```bash
sudo git clone https://github.com/mattloper/opendr.git
cd opendr
python setup.py build
python setup.py install
```

Install OpenCV:
```bash
pip install opencv-python==4.2.0.32
```

Set the PYTHONPATH:
```bash
export PYTHONPATH=/home/.../src/SMPL_python_v.1.1.0/smpl/
```

### 4. Additional Steps for OpenDR
Refer to the [issue](https://github.com/microsoft/MeshTransformer/issues/35):
1. Clone the OpenDR repository and checkout to version 0.78:
    ```bash
    git clone https://github.com/mattloper/opendr.git
    cd opendr
    git checkout v0.78
    ```

2. Download `OSMesa.Linux.x86_64.zip` and place it in the `opendr/contexts` directory:
    [Download OSMesa.Linux.x86_64.zip](http://files.is.tue.mpg.de/mloper/opendr/osmesa/OSMesa.Linux.x86_64.zip)

3. Modify the import line in `opendr/contexts/ctx_base.pyx`:
    ```python
    from opendr.contexts._constants import *
    ```

4. Build and install OpenDR:
    ```bash
    python setup.py build
    python setup.py install
    ```

### 5. Serialization Issue Fix
In `serialization.py`, update the pickle load method:
```python
# dd = pickle.load(open(fname_or_dict))
dd = pickle.load(open(fname_or_dict, "rb"))
```

### 6. WSL with GUI Support
For GUI applications on WSL, follow these steps:
1. Export the display:
    ```bash
    export DISPLAY=$(cat /etc/resolv.conf | grep nameserver | awk '{print $2}'):0
    ```

2. Update WSL:
    ```bash
    wsl --update
    export DISPLAY=:0
    ```

Refer to this [tutorial](https://learn.microsoft.com/ko-kr/windows/wsl/tutorials/gui-apps) for more details.

## Additional Resources
- [WSL + GUI](https://novelism.co.kr/273)
