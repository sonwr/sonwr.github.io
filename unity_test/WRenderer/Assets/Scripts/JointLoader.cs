using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JointLoader : MonoBehaviour
{
    // Prefab
    public GameObject jointPrefab;  // Prefab for the sphere to represent joints
    public GameObject bonePrefab;   // Prefab for the cylinder to represent bones

    // Parameter
    public int frameIndex = 2100;//1800;
    public int frameStartIndex = 2100;//1800;
    public int frameLastIndex = 2350;//3600; //2399;  // 3600;

    // Body Model
    private List<BodyData> modelList;

    private string[] modelNames = new string[] { "DeepRobot", "SMPLify", "GroundTruth" };
    private Color[] modelColors = new Color[] { Color.blue, Color.red, Color.yellow };

    // Start is called before the first frame update
    void Start()
    {
        InitModelList();

        frameIndex = frameStartIndex;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < modelList.Count; i++)
            modelList[i].Render(frameIndex, frameStartIndex, frameLastIndex);

        frameIndex++;
        if (frameIndex > frameLastIndex)
            frameIndex = frameStartIndex;
    }

    void InitModelList()
    {
        modelList = new List<BodyData>();

        for (int i = 0; i < modelNames.Length; i++)
        {
            string modelname = modelNames[i];
            Color modelColor = modelColors[i];

            // Create Body GameObject
            GameObject emptyChildGameObject = new GameObject(modelname);
            emptyChildGameObject.transform.SetParent(this.transform, false);

            // Init Body Model
            BodyData bodyData = new BodyData(modelname);
            bodyData.Init(jointPrefab, bonePrefab, emptyChildGameObject, modelColor);
            modelList.Add(bodyData);

            // DeepRobot
            if (i == 0)
                bodyData.LoadFileDeepRobot(frameStartIndex, frameLastIndex);
        }
    }
}
