# Stacked Hourglass Networks in Pytorch

Requirements:
- Python 3 (code has been tested on Python 3.8.2)
- PyTorch (code tested with 1.5)
- CUDA and cuDNN (tested with Cuda 10)
- Python packages (not exhaustive): opencv-python (tested with 4.2), tqdm, cffi, h5py, scipy (tested with 1.4.1), pytz, imageio

## 1. Python 3 (code has been tested on Python 3.8.2)
```
> pyenv versions
> pyenv install 3.8.2
> pyenv global 3.8.2
> pip install --upgrade pip
> pip install virtualenv
```

## 2. virtualenv
```
> (move to working dir)
> virtualenv py382
> cd .\py382\Scripts\
> .\activate
```

## 3. PyTorch (code tested with 1.5)
https://developer.nvidia.com/cuda-10.2-download-archive?target_os=Windows&target_arch=x86_64&target_version=10&target_type=exelocal
https://pytorch.kr/get-started/previous-versions/
```
# CUDA 10.2
> pip install torch==1.5.0 torchvision==0.6.0
> pip install torch==1.5.0 torchvision==0.6.0 -f https://download.pytorch.org/whl/torch_stable.html
```

## 4. Dataset
Download the full [MPII Human Pose dataset](http://human-pose.mpi-inf.mpg.de/), and place the images directory in data/MPII/





