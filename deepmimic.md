# DeepMimic Windows 컴파일 안내

## 컴파일 환경
- **운영체제**: Windows 11 Pro
- **개발 환경**: Visual Studio 2019
- **소스코드 위치**: [DeepMimic GitHub 페이지](https://github.com/xbpeng/DeepMimic)
- **Python Version**: 3.7.4

## 필요한 외부 라이브러리
1. **Eigen**: [다운로드 링크](http://eigen.tuxfamily.org/)
2. **Freeglut**: `freeglut-MSVC-3.0.0-2.mp.zip`
3. **Glew**: `glew-2.1.0-win32.zip`
4. **MS MPI 10.0**:
   - MS MPI 10.1.3가 아닌 10.0 설치
   - 설치 후 `Pip install mpi4py` 실행
5. **Swig**: [다운로드 링크](https://www.swig.org/download.html) (Windows prebuilt executable)

## Windows 환경 변수 Path 설정
1. `(FREEGLUT_PATH)/freeglut-MSVC-3.0.0-2.mp\freeglut\bin\x64`
2. `(GLEW_PATH)/glew-2.1.0\bin\Release\x64`
3. `(SWIGWIN_PATH)/swigwin-4.1.1`
4. `C:\Program Files\Microsoft MPI\Bin`
5. `C:\Program Files (x86)\Microsoft SDKs\MPI\Lib`
6. `C:\Program Files (x86)\Microsoft SDKs\MPI`

## 컴파일 에러 해결 방법
- **Assert 에러**: 
  - `identifier not found` 에러 발생 시, 헤더 파일에 다음 코드 추가:
    ```cpp
    #include <assert.h>
    #include <cassert>
    ```
- **M_PI 에러**: 
  - 헤더 파일에 다음 코드 추가:
    ```cpp
    #define _USE_MATH_DEFINES
    #include <cmath>
    ```

## 빌드 방법
- `DeepMimicCore` 프로젝트를 `Release_Swig` 구성으로 빌드
- 빌드 후 `DeepMimicCore.py` 파일이 `DeepMimicCore/` 디렉토리에 생성됨

![Screenshot 2023-12-23 at 7 53 54 PM](https://github.com/sonwr/sonwr.github.io/assets/4978617/6c0d3490-7dc7-4a2c-9d42-8c827f3f1857)


# DeepMimic Windows Compilation Guide

## Compilation Environment
- **Operating System**: Windows 11 Pro
- **Development Environment**: Visual Studio 2019
- **Source Code Location**: [DeepMimic GitHub Page](https://github.com/xbpeng/DeepMimic)
- **Python Version**: 3.7.4

## Required External Libraries
1. **Eigen**: [Download Link](http://eigen.tuxfamily.org/)
2. **Freeglut**: `freeglut-MSVC-3.0.0-2.mp.zip`
3. **Glew**: `glew-2.1.0-win32.zip`
4. **MS MPI 10.0**:
   - Install MS MPI 10.0, not 10.1.3
   - After installation, run `Pip install mpi4py`
5. **Swig**: [Download Link](https://www.swig.org/download.html) (Windows prebuilt executable)

## Setting Windows Environment Variables Path
1. `(FREEGLUT_PATH)/freeglut-MSVC-3.0.0-2.mp\freeglut\bin\x64`
2. `(GLEW_PATH)/glew-2.1.0\bin\Release\x64`
3. `(SWIGWIN_PATH)/swigwin-4.1.1`
4. `C:\Program Files\Microsoft MPI\Bin`
5. `C:\Program Files (x86)\Microsoft SDKs\MPI\Lib`
6. `C:\Program Files (x86)\Microsoft SDKs\MPI`

## Compilation Error Solutions
- **Assert Error**: 
  - If `identifier not found` error occurs, add the following code to the header file:
    ```cpp
    #include <assert.h>
    #include <cassert>
    ```
- **M_PI Error**: 
  - Add the following code to the header file:
    ```cpp
    #define _USE_MATH_DEFINES
    #include <cmath>
    ```

## Build Instructions
- Build the `DeepMimicCore` project with the `Release_Swig` configuration
- After the build, `DeepMimicCore.py` will be generated in the `DeepMimicCore/` directory
