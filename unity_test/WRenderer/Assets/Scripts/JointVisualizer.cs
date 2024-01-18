using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

public enum JOINT_IDX_2D
{
    CAPSKEL_Neck = 0,
    CAPSKEL_RShoulder = 1,
    CAPSKEL_LShoulder = 2,
    CAPSKEL_RHip = 3,
    CAPSKEL_LHip = 4,
    CAPSKEL_RElbow = 5,
    CAPSKEL_LElbow = 6,
    CAPSKEL_RKnee = 7,
    CAPSKEL_LKnee = 8,
    CAPSKEL_RWrist = 9,
    CAPSKEL_LWrist = 10,
    CAPSKEL_RAnkle = 11,
    CAPSKEL_LAnkle = 12
};

public enum JOINT_IDX_3D
{
    CAPSKEL_Neck = 0,
    CAPSKEL_Head = 1,
    CAPSKEL_RShoulder = 2,
    CAPSKEL_RElbow = 3,
    CAPSKEL_RWrist = 4,
    CAPSKEL_LShoulder = 5,
    CAPSKEL_LElbow = 6,
    CAPSKEL_LWrist = 7,
    CAPSKEL_RHip = 8,
    CAPSKEL_RKnee = 9,
    CAPSKEL_RAnkle = 10,
    CAPSKEL_LHip = 11,
    CAPSKEL_LKnee = 12,
    CAPSKEL_LAnkle = 13,
};

public enum JOINT_MODEL_TYPE
{
    DEEPROBOT,
    SMPLIFY
}


[System.Serializable]
public class JointData
{
    public List<List<float>> jointPositions;
}

public class JointVisualizer : MonoBehaviour
{
    // Prefab for the sphere to represent joints
    public GameObject jointPrefab;

    // Prefab for the cylinder to represent bones
    public GameObject bonePrefab;

    // Prefab for the cylinder to represent ray
    public GameObject rayPrefab;

    public JointVisualizer jointVisualizerDeepRobot;

    // CameraSensor 스크립트가 부착된 GameObject에 대한 참조
    public CameraSensor cameraSensor;

    public JOINT_MODEL_TYPE jointModelType = JOINT_MODEL_TYPE.DEEPROBOT;

    public float scale = 0.01f;


    // Internal use
    public int frameIndex = 1800;

    public List<GameObject> jointObjects3D;

    // Path to the JSON file within the Assets folder
    private string filePath = Directory.GetCurrentDirectory() + "/Data/deeprobot_joint_3d.json";

    // The parent index for each joint
    private int[] jointParentIndex = new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };


    // Start is called before the first frame update
    void Start()
    {
        if (jointModelType == JOINT_MODEL_TYPE.DEEPROBOT)
        {
            //filePath = Directory.GetCurrentDirectory() + "/Data/deeprobot_joint_3d.json

            string fileName = "f_" + frameIndex.ToString() + "_3_joint_3d.json";
            filePath = Directory.GetCurrentDirectory() + "/Data/" + fileName;
            jointParentIndex = new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };

            // Draw Ray (2D to 3D Joint)
            Invoke("CreateRayFunctionToInvoke", 2.0f);
        }
        else
        {
            //filePath = Directory.GetCurrentDirectory() + "/Data/smplify_joint_3d.json";

            string fileName = "f_" + frameIndex.ToString() + "_3_joint_3d_smplify4.json";
            filePath = Directory.GetCurrentDirectory() + "/Data/" + fileName;
            jointParentIndex = new int[] { 3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 12, 12, 24, 13, 14, 16, 17, 18, 19, 20, 21, 24 };

            // Align (DeepRobot, SMPLify)
            Invoke("AlignmentFunctionToInvoke", 2.0f);
        }

        // Read and process the JSON file
        string dataAsJson = File.ReadAllText(filePath);
        var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

        // Create a list to store the instantiated joint GameObjects
        jointObjects3D = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < joints.Length; i++)
        {
            float x = joints[i][0] * scale;
            float y = joints[i][1] * scale;
            float z = joints[i][2] * scale;

            Vector3 jointPosition = new Vector3(x, y, z);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.transform.SetParent(this.transform);

            jointObjects3D.Add(joint);
        }
    }


    void CreateRayFunctionToInvoke()
    {
        // Create bones between joints
        for (int i = 0; i < jointObjects3D.Count; i++)
        {
            int parentIdx = jointParentIndex[i];
            if (parentIdx < jointObjects3D.Count)
                CreateBoneBetweenJoints(jointObjects3D[i], jointObjects3D[jointParentIndex[i]]);
        }

        CreateRayBetweenJoints(jointObjects3D[0], cameraSensor.jointObjects2D[1]);  // Neck
        CreateRayBetweenJoints(jointObjects3D[1], cameraSensor.jointObjects2D[2]);  // Right Shoulder
        CreateRayBetweenJoints(jointObjects3D[2], cameraSensor.jointObjects2D[5]);  // Left Shoulder
        CreateRayBetweenJoints(jointObjects3D[3], cameraSensor.jointObjects2D[8]);  // Right Hip
        CreateRayBetweenJoints(jointObjects3D[4], cameraSensor.jointObjects2D[11]);  // Left Hip
        CreateRayBetweenJoints(jointObjects3D[5], cameraSensor.jointObjects2D[3]);  // Right Elbow
        CreateRayBetweenJoints(jointObjects3D[6], cameraSensor.jointObjects2D[6]);  // Left Elbow
        CreateRayBetweenJoints(jointObjects3D[7], cameraSensor.jointObjects2D[9]);  // Right Knee
        CreateRayBetweenJoints(jointObjects3D[8], cameraSensor.jointObjects2D[12]);  // Left Knee
        CreateRayBetweenJoints(jointObjects3D[9], cameraSensor.jointObjects2D[4]);  // Right Knee
        CreateRayBetweenJoints(jointObjects3D[10], cameraSensor.jointObjects2D[7]);  // Left Knee
        CreateRayBetweenJoints(jointObjects3D[11], cameraSensor.jointObjects2D[10]);  // Right Ankle
        CreateRayBetweenJoints(jointObjects3D[12], cameraSensor.jointObjects2D[13]);  // Left Ankle

        /*
        // 3D 조인트와 2D 조인트를 연결하는 선 생성
        for (int i = 0; i < jointObjects3D.Count; i++)
        {
            if (i + 1 < cameraSensor.jointObjects2D.Count)
            {
                CreateRayBetweenJoints(jointObjects3D[i], cameraSensor.jointObjects2D[i+1]);
            }
        }
        */
    }

    void AlignmentFunctionToInvoke()
    {
        // 좌표 보정
        AdjustScaleAndPosition(jointObjects3D, jointVisualizerDeepRobot.jointObjects3D);

        for (int i = 0; i < jointObjects3D.Count; i++)
        {
            int parentIdx = jointParentIndex[i];
            if (parentIdx < jointObjects3D.Count)
                CreateBoneBetweenJoints(jointObjects3D[i], jointObjects3D[jointParentIndex[i]]);
        }
    }

    private float CalculateDistance(GameObject point1, GameObject point2)
    {
        return Vector3.Distance(point1.transform.position, point2.transform.position);
    }

    private void AdjustScaleAndPosition(List<GameObject> smplifyJoints, List<GameObject> robotJoints)
    {
        // 스케일 계산
        float robotDistance = CalculateDistance(robotJoints[(int)JOINT_IDX_3D.CAPSKEL_RShoulder], robotJoints[(int)JOINT_IDX_3D.CAPSKEL_LShoulder]);
        float smplifyDistance = CalculateDistance(smplifyJoints[16], smplifyJoints[17]); // 인덱스는 Python 코드에 기반하여 조정해야 할 수 있음
        float scale = robotDistance / smplifyDistance;

        // 스케일 조정
        for (int i = 0; i < smplifyJoints.Count; i++)
        {
            smplifyJoints[i].transform.localScale *= scale;
        }

        // 위치 조정
        Vector3 robotPoint = robotJoints[(int)JOINT_IDX_3D.CAPSKEL_Neck].transform.position;
        Vector3 smplifyPoint = smplifyJoints[12].transform.position; // 인덱스는 Python 코드에 기반하여 조정해야 할 수 있음
        Vector3 displacement = robotPoint - smplifyPoint;

        for (int i = 0; i < smplifyJoints.Count; i++)
        {
            smplifyJoints[i].transform.position += displacement;
        }
    }

    // Method to create a bone between two joints
    private void CreateBoneBetweenJoints(GameObject childJoint, GameObject parentJoint)
    {
        Vector3 direction = childJoint.transform.position - parentJoint.transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        GameObject bone = Instantiate(bonePrefab);
        bone.transform.position = parentJoint.transform.position + direction * distance * 0.5f;
        bone.transform.up = direction;
        bone.transform.localScale = new Vector3(0.05f, distance * 0.5f, 0.05f);
        bone.transform.SetParent(this.transform);
    }

    // Method to create a bone between two joints
    private void CreateRayBetweenJoints(GameObject childJoint, GameObject parentJoint)
    {
        Vector3 direction = childJoint.transform.position - parentJoint.transform.position;
        float distance = direction.magnitude;
        direction.Normalize();

        GameObject bone = Instantiate(rayPrefab);
        bone.transform.position = parentJoint.transform.position + direction * distance * 0.5f;
        bone.transform.up = direction;
        bone.transform.localScale = new Vector3(0.0005f, distance * 0.5f, 0.0005f);
        bone.transform.SetParent(this.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
