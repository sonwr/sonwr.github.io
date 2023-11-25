# Openpose 컴파일 및 문제 해결
작성일: 2023년 11월 25일

## 소스코드 버전 및 커밋
- Openpose 소스코드: [80d4c5f](https://github.com/CMU-Perceptual-Computing-Lab/openpose/commit/80d4c5f7b25ba4c3bf5745ab7d0e6ccd3db8b242)

## 컴파일 환경
- OS: Windows 11
- IDE: Visual Studio 2019
- GPU: CUDA 11.2
- CMake 버전: 3.28.0-rc5
- CUDA 버전: 11.1.1
- cuDNN 버전: 8.1.0

## 설치 과정
1. Openpose 설치 문서: [OpenPose Doc - Installation](https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/installation/0_index.md#windows-portable-demo)
2. 참고 설치 가이드: [OpenPose 개요 및 설치하기](https://velog.io/@oneul1213/OpenPose-%EA%B0%9C%EC%9A%94-%EB%B0%8F-%EC%84%A4%EC%B9%98%ED%95%98%EA%B8%B0)

## 문제 해결

### 1. 3rdparty/windows/ 디렉토리 안에 있는 .bat 파일 실행 시
   - 문제: zip 파일 다운로드 불가능
   - 해결: [GitHub 이슈 #1602](https://github.com/CMU-Perceptual-Computing-Lab/openpose/issues/1602) 참조
   - 다운로드 링크:
     - Models: [Models 다운로드](https://drive.google.com/file/d/1QCSxJZpnWvM00hx49CJ2zky7PWGzpcEh)
     - 3rdparty before 2021: [3rdparty (2021 이전 버전) 다운로드](https://drive.google.com/file/d/1mqPEnqCk5bLMZ3XnfvxA4Dao7pj0TErr)
     - 3rdparty for 2021 versions: [3rdparty (2021 버전) 다운로드](https://drive.google.com/file/d/1WvftDLLEwAxeO2A-n12g5IFtfLbMY9mG)

## 관련 링크
1. [Openpose GitHub Repository](https://github.com/sonwr/openpose)
2. [Openpose 설치 문서](https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/installation/0_index.md#windows-portable-demo)
3. [Velog Openpose 설치 포스트](https://velog.io/@oneul1213/OpenPose-%EA%B0%9C%EC%9A%94-%EB%B0%8F-%EC%84%A4%EC%B9%98%ED%95%98%EA%B8%B0)
4. [GitHub 이슈 #1602](https://github.com/CMU-Perceptual-Computing-Lab/openpose/issues/1602)


# Compile and Troubleshoot Openpose
Date: November 25, 2023

## Source Code Version and Commit
- Openpose Source Code: [80d4c5f](https://github.com/CMU-Perceptual-Computing-Lab/openpose/commit/80d4c5f7b25ba4c3bf5745ab7d0e6ccd3db8b242)

## Compilation Environment
- OS: Windows 11
- IDE: Visual Studio 2019
- GPU: CUDA 11.2
- CMake Version: 3.28.0-rc5
- CUDA Version: 11.1.1
- cuDNN Version: 8.1.0

## Installation Process
1. Openpose Installation Documentation: [OpenPose Doc - Installation](https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/installation/0_index.md#windows-portable-demo)
2. Reference Installation Guide: [OpenPose Overview and Installation](https://velog.io/@oneul1213/OpenPose-%EA%B0%9C%EC%9A%94-%EB%B0%8F-%EC%84%A4%EC%B9%98%ED%95%98%EA%B8%B0)

## Troubleshooting

### 1. When running .bat files in the 3rdparty/windows/ directory
   - Issue: Unable to download zip files
   - Solution: Refer to [GitHub Issue #1602](https://github.com/CMU-Perceptual-Computing-Lab/openpose/issues/1602)
   - Download Links:
     - Models: [Download Models](https://drive.google.com/file/d/1QCSxJZpnWvM00hx49CJ2zky7PWGzpcEh)
     - 3rdparty before 2021: [Download 3rdparty (pre-2021 version)](https://drive.google.com/file/d/1mqPEnqCk5bLMZ3XnfvxA4Dao7pj0TErr)
     - 3rdparty for 2021 versions: [Download 3rdparty (2021 version)](https://drive.google.com/file/d/1WvftDLLEwAxeO2A-n12g5IFtfLbMY9mG)

## Related Links
1. [Openpose GitHub Repository](https://github.com/sonwr/openpose)
2. [Openpose Installation Documentation](https://github.com/CMU-Perceptual-Computing-Lab/openpose/blob/master/doc/installation/0_index.md#windows-portable-demo)
3. [Velog Openpose Installation Post](https://velog.io/@oneul1213/OpenPose-%EA%B0%9C%EC%9A%94-%EB%B0%8F-%EC%84%A4%EC%B9%98%ED%95%98%EA%B8%B0)
4. [GitHub Issue #1602](https://github.com/CMU-Perceptual-Computing-Lab/openpose/issues/1602)

