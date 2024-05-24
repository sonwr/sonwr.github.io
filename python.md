# Python
Last updated: 15 Sept 2023

## 1. pyenv 
```
> pyenv --version
> pyenv install -l
> pyenv versions
```
```
> pyenv install 3.9.5
> pyenv uninstall 3.9.x
> pyenv global 3.9.5
> pip install --upgrade pip
> pip install virtualenv
```

Troubleshooting (osx): [^1]
> WARNING: The Python lzma extension was not compiled. Missing the lzma lib? 
```
> brew install xz
```


## 2. virtualenv
```
> virtualenv [name]
> virtualenv py39

(windows)
> activate.bat
> deactivate.bat
> ..\Python\py39\Scripts\activate

(osx)
% source activate

(linux, python 2.7.18)
% virtualenv --python=python2.7.18 py2718
```

## 3. pytorch
- https://pytorch.kr/get-started/locally/#windows-prerequisites
```
(windows)
> pip3 install torch torchvision torchaudio --index-url https://download.pytorch.org/whl/cu118

(osx)
% pip3 install torch torchvision torchaudio
```


## Reference
[^1]: https://mojaie.github.io/pyenv-lzma/




