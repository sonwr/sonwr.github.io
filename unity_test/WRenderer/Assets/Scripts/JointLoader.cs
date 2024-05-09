using System;
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
    public int frameIndex = 0; //2100;//1800;
    public int frameStartIndex = 0; //2100;//1800;
    public int frameLastIndex = 1300; //2350;//3600; //2399;  // 3600;

    // Body Model
    private List<BodyData> modelList;

    private BodyType[] bodyModels = new BodyType[] { BodyType.DeepRobot, BodyType.SMPLify, BodyType.GroundTruth };
    //private BodyType[] bodyModels = new BodyType[] { BodyType.DeepRobot, BodyType.GroundTruth };

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
            //if (bodyType == BodyType.DeepRobot)
            //    smplBodyGameObject.transform.Rotate(0, 180, 0);

            jointBodyGameObject.name = bodyData.GetModelName();
            smplBodyGameObject.name = bodyData.GetModelName() + "_SMPL";

            modelList.Add(bodyData);
        }


        // Statistics
        float mpjpe = Util.CalculateMPJPE(modelList[0].GetJointFrameList(), modelList[2].GetJointFrameList());
        float mpjre = Util.CalculateMPJRE(modelList[0].GetJointFrameList(), modelList[2].GetJointFrameList());
        float roote = Util.CalculateRootE(modelList[0].GetJointFrameList(), modelList[2].GetJointFrameList());

        float tjitterDeepRobot = Util.CalculateTemporalJointJitter(modelList[0].GetJointFrameList());
        float tjitterGT = Util.CalculateTemporalJointJitter(modelList[2].GetJointFrameList());

        // Writing statistics to the file
        try
        {
            string filepath = Directory.GetCurrentDirectory() + "/Data/result.txt";

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create a file to write to
            using (StreamWriter writer = new StreamWriter(filepath))
            {
                writer.WriteLine("MPJPE: " + mpjpe);
                writer.WriteLine("MPJRE: " + mpjre);
                writer.WriteLine("RootE: " + roote);
                writer.WriteLine("Temporal Joint Jitter (DeepRobot): " + tjitterDeepRobot);
                writer.WriteLine("Temporal Joint Jitter (GT): " + tjitterGT);
            }

            Console.WriteLine("Statistics saved successfully to " + filepath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error writing to file: " + e.Message);
        }


        // Align (SMPLify <-> DeepRobot)
        float alignScale = 1.0f;
        Vector3 alignTransform = new Vector3();

        (alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[1]);
        //modelList[1].SetScaleAndDisplacement(alignScale, alignTransform);


        // Align (SMPLify <-> Ground Truth)
        (alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[2]);
        //modelList[2].SetScaleAndDisplacement(alignScale, alignTransform);
    }
}
