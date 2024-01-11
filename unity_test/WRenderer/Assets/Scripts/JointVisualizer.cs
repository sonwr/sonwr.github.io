using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

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


    // Path to the JSON file within the Assets folder
    private string filePath = Directory.GetCurrentDirectory() + "/Data/deeprobot_joint_3d.json";

    // The parent index for each joint
    private int[] jointParentRobot = new int[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };


    // Start is called before the first frame update
    void Start()
    {
        // Read and process the JSON file
        string dataAsJson = File.ReadAllText(filePath);
        var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

        // Create a list to store the instantiated joint GameObjects
        List<GameObject> jointObjects = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < joints.Length; i++)
        {
            float scale = 100.0f;
            float x = joints[i][0] / scale;
            float y = joints[i][1] / scale;
            float z = joints[i][2] / scale;

            Vector3 jointPosition = new Vector3(x, y, z);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.transform.SetParent(this.transform);

            jointObjects.Add(joint);
        }

        // Create bones between joints
        for (int i = 1; i < jointObjects.Count; i++)
        {
            CreateBoneBetweenJoints(jointObjects[i], jointObjects[jointParentRobot[i]]);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
