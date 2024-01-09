import pybullet as p
import time
import pybullet_data

physicsClient = p.connect(p.GUI)  # GUI로 PyBullet 시작
p.setAdditionalSearchPath(pybullet_data.getDataPath())  # 데이터 경로 설정

p.setGravity(0, 0, -10)  # 중력 설정
planeId = p.loadURDF("plane.urdf")  # 평면 로딩
robotId = p.loadURDF("r2d2.urdf", basePosition=[0, 0, 1])  # 캐릭터 모델 로딩

for i in range(10000):
    p.stepSimulation()  # 시뮬레이션 한 단계 진행
    time.sleep(1./240.)  # 실시간 시뮬레이션 속도 조절

p.disconnect()  # PyBullet 연결 해제

