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
    // Path to the JSON file within the Assets folder
    private string filePath = Directory.GetCurrentDirectory() + "/Data/deeprobot_joint_3d.json";

    // Prefab for the sphere to represent joints
    public GameObject jointPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //GameObject newObject = Instantiate(jointPrefab, new UnityEngine.Vector3(-1, -1, 1), UnityEngine.Quaternion.identity);
        //newObject.transform.SetParent(this.transform);

        // Read and process the JSON file
        string dataAsJson = File.ReadAllText(filePath);
        var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

        // 각 조인트의 좌표 출력
        for (int i = 0; i < joints.Length; i++)
        {
            float x = joints[i][0];
            float y = joints[i][1];
            float z = joints[i][2];

            Vector3 jointPosition = new Vector3(x, y, z);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.transform.SetParent(this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
