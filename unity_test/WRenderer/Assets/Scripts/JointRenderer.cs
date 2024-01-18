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

    public float scaleFactorDeepRobot = 0.01f;
    public float scaleFactorSMPLify = 0.01f;

    // The parent index for each joint
    private int[] jointParentIndexDeepRobot = new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    private int[] jointParentIndexSMPLify = new int[] { 3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 12, 12, 24, 13, 14, 16, 17, 18, 19, 20, 21, 24 };

    // Start is called before the first frame update
    void Start()
    {
        InitData();
        DataLoader();
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

    private void DataLoader()
    {
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
                float x = joints[i][0] * scaleFactorDeepRobot;
                float y = joints[i][1] * scaleFactorDeepRobot;
                float z = joints[i][2] * scaleFactorDeepRobot;

                if (i < 0 || i >= jointGameObjectsDeepRobot.Count)
                    continue;

                jointGameObjectsDeepRobot[i].transform.position = new Vector3(x, y, z);
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
                float x = joints[i][0] * scaleFactorSMPLify;
                float y = joints[i][1] * scaleFactorSMPLify;
                float z = joints[i][2] * scaleFactorSMPLify;

                if (i < 0 || i >= jointGameObjectsSMPLify.Count)
                    continue;

                jointGameObjectsSMPLify[i].transform.position = new Vector3(x, y, z);
            }
        }
    }            
}
