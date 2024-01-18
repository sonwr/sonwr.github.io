using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;

public class JointRenderer : MonoBehaviour
{
    // Prefab for the sphere to represent joints
    public GameObject jointPrefab;
    public GameObject jointsParentDeepRobot;
    public GameObject jointsParentSMPLify;

    // Joint
    public List<GameObject> jointGameObjectsDeepRobot;
    public List<GameObject> jointGameObjectsSMPLify;

    // Internal use
    private int frameIndex = 1800;
    private int frameLastIndex = 3600;

    private int jointCountDeepRobot = 13;
    private int jointCountSMPLify = 24;

    private float scaleFactorDeepRobot = 0.01f;

    private float alignScaleFactor = 1.0f;
    private Vector3 alignTransform = new Vector3();

    // The parent index for each joint
    private int[] jointParentIndexDeepRobot = new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    private int[] jointParentIndexSMPLify = new int[] { 3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 12, 12, 24, 13, 14, 16, 17, 18, 19, 20, 21, 24 };

    // Start is called before the first frame update
    void Start()
    {
        InitData();
        DataLoader(true);
        DataLoader(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitData()
    {
        // DeepRobot Joints - Create a list to store the instantiated joint GameObjects
        jointGameObjectsDeepRobot = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < jointCountDeepRobot; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.transform.SetParent(jointsParentDeepRobot.transform);
            jointGameObjectsDeepRobot.Add(joint);
        }

        // SMPLify Joints - Create a list to store the instantiated joint GameObjects
        jointGameObjectsSMPLify = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < jointCountSMPLify; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.transform.SetParent(jointsParentSMPLify.transform);
            jointGameObjectsSMPLify.Add(joint);
        }
    }

    private void DataLoader(bool calcAlign)
    {
        List<Vector3> jointsListDeepRobot = new List<Vector3>();
        List<Vector3> jointsListSMPLify = new List<Vector3>();

        // DeepRobot Joints
        if (jointGameObjectsDeepRobot.Count >= jointCountDeepRobot)
        {
            string filePath = Directory.GetCurrentDirectory() + "/Data/f_" + frameIndex.ToString() + "_3_joint_3d.json";

            // Read and process the JSON file
            string dataAsJson = File.ReadAllText(filePath);
            var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

            // Instantiate joints and store them in the list
            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 position = new Vector3(joints[i][0], joints[i][1], joints[i][2]);
                jointsListDeepRobot.Add(position);

                // GameObject
                jointGameObjectsDeepRobot[i].transform.position = position * scaleFactorDeepRobot;
            }
        }

        // SMPLify Joints
        if (jointGameObjectsSMPLify.Count >= jointCountSMPLify)
        {
            string filePath = Directory.GetCurrentDirectory() + "/Data/f_" + frameIndex.ToString() + "_3_joint_3d_smplify4.json";

            // Read and process the JSON file
            string dataAsJson = File.ReadAllText(filePath);
            var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

            // Instantiate joints and store them in the list
            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 position = new Vector3(joints[i][0], joints[i][1], joints[i][2]);
                jointsListSMPLify.Add(position);

                // Alignment
                Vector3 newPosition = (position * alignScaleFactor) + alignTransform;

                // GameObject
                jointGameObjectsSMPLify[i].transform.position = newPosition * scaleFactorDeepRobot;
            }
        }

        // Calculate Alignment
        if (calcAlign == true)
            (alignScaleFactor, alignTransform) = AdjustScaleAndPosition(jointsListDeepRobot, jointsListSMPLify);
    }

    /*
    private void DataLoader2()
    {
        // Load Json
        string filePath = Directory.GetCurrentDirectory() + "/Data/f_" + frameIndex.ToString() + "_3_joint_3d.json";
        string dataAsJson = File.ReadAllText(filePath);
        var jointsDeepRobot = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

        filePath = Directory.GetCurrentDirectory() + "/Data/f_" + frameIndex.ToString() + "_3_joint_3d_smplify4.json";
        dataAsJson = File.ReadAllText(filePath);
        var jointsSMPLify = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

        // Align
        AdjustScaleAndPosition(jointsDeepRobot, jointsSMPLify);


        // DeepRobot Joints
        if (jointGameObjectsDeepRobot.Count >= jointCountDeepRobot)
        {
            for (int i = 0; i < jointsDeepRobot.Length; i++)
            {
                float x = jointsDeepRobot[i][0] * scaleFactorDeepRobot;
                float y = jointsDeepRobot[i][1] * scaleFactorDeepRobot;
                float z = jointsDeepRobot[i][2] * scaleFactorDeepRobot;

                if (i < 0 || i >= jointGameObjectsDeepRobot.Count)
                    continue;

                jointGameObjectsDeepRobot[i].transform.position = new Vector3(x, y, z);
            }
        }

        // SMPLify Joints
        if (jointGameObjectsSMPLify.Count >= jointCountSMPLify)
        {
            for (int i = 0; i < jointsSMPLify.Length; i++)
            {
                float x = jointsSMPLify[i][0] * scaleFactorSMPLify;
                float y = jointsSMPLify[i][1] * scaleFactorSMPLify;
                float z = jointsSMPLify[i][2] * scaleFactorSMPLify;

                if (i < 0 || i >= jointGameObjectsSMPLify.Count)
                    continue;

                jointGameObjectsSMPLify[i].transform.position = new Vector3(x, y, z);
            }
        }
    }
    */

    private (float, Vector3) AdjustScaleAndPosition(List<Vector3> jointsListDeepRobot, List<Vector3> jointsListSMPLify)
    {
        // 스케일 계산
        float robotDistance = Vector3.Distance(jointsListDeepRobot[(int)JOINT_IDX_3D.CAPSKEL_RShoulder], jointsListDeepRobot[(int)JOINT_IDX_3D.CAPSKEL_LShoulder]);
        float smplifyDistance = Vector3.Distance(jointsListSMPLify[16], jointsListSMPLify[17]);
        float scale = robotDistance / smplifyDistance;



        // 위치 조정
        Vector3 robotPoint = jointsListDeepRobot[(int)JOINT_IDX_3D.CAPSKEL_Neck];
        Vector3 smplifyPoint = jointsListSMPLify[12] * scale;
        Vector3 displacement = robotPoint - smplifyPoint;

        return (scale, displacement);
    }
}
