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

    private BodyType[] bodyModels = new BodyType[] { BodyType.DeepRobot, BodyType.SMPLify, BodyType.GroundTruth };

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

        for (int i = 0; i < bodyModels.Length; i++)
        {
            GameObject emptyChildGameObject = new GameObject();
            emptyChildGameObject.transform.SetParent(this.transform, false);

            BodyType bodyType = bodyModels[i];
            BodyData bodyData = new BodyData(bodyType);
            bodyData.Init(jointPrefab, bonePrefab, emptyChildGameObject);
            bodyData.LoadFile(frameStartIndex, frameLastIndex);

            emptyChildGameObject.name = bodyData.GetModelName();
            modelList.Add(bodyData);
        }


        // Align
        float alignScale = 1.0f;
        Vector3 alignTransform = new Vector3();

        (alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[1]);
        modelList[1].SetScaleAndDisplacement(alignScale, alignTransform);

    }
}
