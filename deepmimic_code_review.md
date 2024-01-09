## DeepMimic.py 코드 분석

이 코드는 OpenGL과 여러 파이썬 라이브러리를 사용하여 DeepMimic 환경에서 시각화와 시뮬레이션을 관리합니다.

### 1. 라이브러리 임포트
- 기본 라이브러리: `numpy`, `sys`, `random`
- OpenGL 라이브러리: `OpenGL.GL`, `OpenGL.GLUT`, `OpenGL.GLU`
- DeepMimic 프로젝트의 사용자 정의 클래스 및 유틸리티: `DeepMimicEnv`, `RLWorld`, `ArgParser`, `Logger` 등

### 2. 전역 변수 설정
- 창 크기 (`win_width`, `win_height`)
- 애니메이션 관련 변수 (`fps`, `update_timestep`, `animating`)
- 재생 속도 (`playback_speed`)
- FPS 계산을 위한 변수 (`prev_time`, `updates_per_sec`)

### 3. 함수 정의
- `build_arg_parser(args)`: 명령줄 인수 파싱
- `update_intermediate_buffer()`, `update_world(world, time_elapsed)`: 시뮬레이션 상태 업데이트
- `draw()`, `reshape(w, h)`: OpenGL을 사용한 그래픽 처리 및 창 크기 변경
- `step_anim(timestep)`, `reload()`, `reset()`: 애니메이션 및 시뮬레이션 관리
- `shutdown()`: 프로그램 종료
- `keyboard(key, x, y)`, `mouse_click(button, state, x, y)`, `mouse_move(x, y)`: 키보드 및 마우스 입력 처리

### 4. `main()` 함수
- 프로그램의 진입점
- 전역 `args` 설정, 그리기 초기화, 재로드, 설정 및 메인 루프 시작

### 5. OpenGL을 사용한 그래픽 처리
- 3D 시뮬레이션 환경 렌더링
- 사용자 입력 처리 및 시뮬레이션 상태 업데이트

### 6. DeepMimic 환경 및 강화 학습
- `DeepMimicEnv`와 `RLWorld` 클래스로 강화 학습 환경 관리
- 에이전트 움직임, 학습 과정, 에피소드 관리

### 7. 사용자 정의 설정과 플레이백
- 명령줄 인수를 통한 설정 조정
- 시뮬레이션의 재생 속도 및 상태 제어



## mpi_run.py 코드 분석

이 코드는 MPI (Message Passing Interface)를 사용하여 DeepMimic 최적화를 병렬 실행하는 스크립트입니다.

### 1. 라이브러리 임포트
- `sys`: 파이썬 기본 시스템 관련 기능
- `subprocess`: 외부 프로세스 실행 및 관리
- 사용자 정의 유틸리티: `ArgParser`, `Logger`
- 최적화 모듈: `DeepMimic_Optimizer`

### 2. `main` 함수
- 명령줄 인수 파싱 및 처리
- `num_workers` 변수로 병렬 처리에 사용될 작업자 수 결정

### 3. 병렬 처리 실행
- `num_workers`가 1보다 크면 MPI를 사용하여 병렬 처리 실행
- `mpiexec` 명령으로 `DeepMimic_Optimizer.py`를 여러 프로세스로 실행
- 명령어는 `subprocess.call`을 통해 실행

### 4. 단일 프로세스 실행
- `num_workers`가 1이면 단일 프로세스에서 `DeepMimic_Optimizer.main()` 호출

### 5. 실행 조건
- 스크립트가 메인 모듈로 실행될 때만 `main()` 함수 실행

이 스크립트는 DeepMimic 프로젝트의 최적화 작업을 병렬로 가속화하는 데 사용됩니다. MPI를 활용하여 고성능 컴퓨팅 환경에서의 효율성을 높입니다.
