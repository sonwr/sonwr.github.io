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
    public GameObject smplPrefab;

    // Parameter
    public int frameIndex = 2100;//1800;
    public int frameStartIndex = 2100;//1800;
    public int frameLastIndex = 2350;//3600; //2399;  // 3600;

    // Body Model
    private List<BodyData> modelList;

    private BodyType[] bodyModels = new BodyType[] { BodyType.DeepRobot, BodyType.SMPLify, BodyType.GroundTruth };

    // Frame
    private float nextActionTime = 0f;
    private float period = 1f / 30f; // 30 FPS에 해당하는 시간 간격

    // Start is called before the first frame update
    void Start()
    {
        InitModelList();

        frameIndex = frameStartIndex;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextActionTime)
        {
            nextActionTime += period;

            for (int i = 0; i < modelList.Count; i++)
                modelList[i].Render(frameIndex, frameStartIndex, frameLastIndex);

            frameIndex++;
            if (frameIndex > frameLastIndex)
                frameIndex = frameStartIndex;
        }
    }

    void InitModelList()
    {
        modelList = new List<BodyData>();

        for (int i = 0; i < bodyModels.Length; i++)
        {
            GameObject jointBodyGameObject = new GameObject();
            jointBodyGameObject.transform.SetParent(this.transform, false);

            // Joint Body
            BodyType bodyType = bodyModels[i];
            BodyData bodyData = new BodyData(bodyType);
            bodyData.Init(jointPrefab, bonePrefab, jointBodyGameObject);
            bodyData.LoadFile(frameStartIndex, frameLastIndex);

            // SMPL Body
            GameObject smplBodyGameObject = new GameObject();
            smplBodyGameObject.transform.SetParent(this.transform, false);
            bodyData.InitSMPL(smplPrefab, smplBodyGameObject);

            jointBodyGameObject.name = bodyData.GetModelName();
            smplBodyGameObject.name = bodyData.GetModelName() + "_SMPL";

            modelList.Add(bodyData);
        }

        // Align (SMPLify <-> DeepRobot)
        float alignScale = 1.0f;
        Vector3 alignTransform = new Vector3();

        (alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[1]);
        modelList[1].SetScaleAndDisplacement(alignScale, alignTransform);


        // Align (SMPLify <-> Ground Truth)
        (alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[2]);
        modelList[2].SetScaleAndDisplacement(alignScale, alignTransform);
    }
}
