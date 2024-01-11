from direct.showbase.ShowBase import ShowBase
from panda3d.core import NodePath, CardMaker, Vec3


class MyApp(ShowBase):
    def __init__(self):
        super().__init__()

        # 환경 설정
        #self.disableMouse()
        #self.camera.setPos(0, -30, 10)
        #self.camera.lookAt(0, 0, 0)

        # 'environment' 노드 생성
        self.environment = NodePath('environment')
        self.environment.reparentTo(self.render)

        # 시각적인 바닥 생성 및 environment의 자식으로 설정
        cm = CardMaker('ground')
        cm.setFrame(-10, 10, -10, 10)  # 바닥의 크기 설정
        ground = NodePath(cm.generate())
        #ground.reparentTo(self.environment)
        ground.setPos(-5, -5, 0)  # 바닥 위치 설정
        ground.setTwoSided(True)

        # 구체 생성 및 environment의 자식으로 설정
        self.ball = self.loader.loadModel("models/misc/sphere")
        self.ball.setScale(2) # 구체 크기 설정
        self.ball.reparentTo(self.environment)
        self.ball.setPos(0, 10, 0)  # 구체 위치 설정
        

        self.environment.setPos(0, 50, -10)  # environment 위치 조정

app = MyApp()
app.run()

