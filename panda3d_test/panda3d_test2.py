from direct.showbase.ShowBase import ShowBase
from panda3d.core import CardMaker, DirectionalLight, AmbientLight, Vec4, Vec3, LineSegs, TextNode
from panda3d.core import LPoint3f, NodePath

class MyApp(ShowBase):
    def __init__(self):
        super().__init__()

        # Add XY plane at z = -1
        self.create_xz_plane()

        # Load a model for the sphere
        self.sphere = self.loader.loadModel("models/misc/sphere")
        self.sphere.reparentTo(self.render)
        self.sphere.setScale(0.5, 0.5, 0.5)  # Scale the sphere
        self.sphere.setPos(0, 0, 1)  # Position the sphere above the floor

        # Add axis lines with labels
        self.create_axis_line_with_label('X', Vec4(1, 0, 0, 1), Vec3(0, 0, 0), Vec3(1, 0, 0))  # X-axis (Red)
        self.create_axis_line_with_label('Y', Vec4(0, 1, 0, 1), Vec3(0, 0, 0), Vec3(0, 1, 0))  # Y-axis (Green)
        self.create_axis_line_with_label('Z', Vec4(0, 0, 1, 1), Vec3(0, 0, 0), Vec3(0, 0, 1))  # Z-axis (Blue)

        # Set the camera position and orientation
        self.camera.setPos(0, 0, -1)  # Camera positioned at (0, 0, -100)
        self.camera.lookAt(0, 0, 0)  # Camera looking towards the origin

    def create_axis_line_with_label(self, label, color, start, end):
        # Draw the line
        line = LineSegs()
        line.setColor(color)
        line.moveTo(start)
        line.drawTo(end)
        line_node = line.create(False)
        self.render.attachNewNode(line_node)

        # Create a text node for the label
        text_node = TextNode('axis_label')
        text_node.setText(label)
        text_node.setAlign(TextNode.ACenter)
        text_node_path = self.render.attachNewNode(text_node)
        text_node_path.setColor(color)
        text_node_path.setPos(LPoint3f(end) + Vec3(0.1, 0.1, 0.1))  # Positioning the label near the end of the line
        text_node_path.setScale(0.2)  # Scale the text to an appropriate size
        text_node_path.setTwoSided(True)

    def create_xz_plane(self):
        # Create a CardMaker for the plane
        cm = CardMaker('xz_plane')
        cm.setFrame(-2, 2, -2, 2)  # Size of the plane
        xz_plane = self.render.attachNewNode(cm.generate())
        xz_plane.setPos(0, 2, 0)  # Position the plane in the YZ plane at y = -1
        xz_plane.setHpr(0, 0, 90)  # Rotate the plane to make it perpendicular to Z-axis
        xz_plane.setColor(Vec4(0.0, 0.5, 0.0, 1))  # Set a grey color
        xz_plane.setTwoSided(True)


app = MyApp()
app.run()
