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
    public int frameLastIndex = 100;//1300; //2350;//3600; //2399;  // 3600;

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
            {
                modelList[i].Render(frameIndex, frameStartIndex, frameLastIndex);
            }

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
            if (bodyType == BodyType.DeepRobot)
                smplBodyGameObject.name = bodyData.GetModelName() + "_SMPL(DeepRobot)";

            modelList.Add(bodyData);
        }

        // Align (SMPLify <-> DeepRobot)
        float alignScale = 1.0f;
        Vector3 alignTransform = new Vector3();

        alignTransform = BodyData.AdjustPositionToHips(modelList[0], modelList[1]);
        modelList[1].SetScaleAndDisplacement(alignScale, alignTransform);

        //(alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[1]);
        //modelList[1].SetScaleAndDisplacement(alignScale, alignTransform);

        // Align (SMPLify <-> Ground Truth)
        (alignScale, alignTransform) = BodyData.AdjustScaleAndPosition(modelList[0], modelList[2]);
        //modelList[2].SetScaleAndDisplacement(alignScale, alignTransform);




        SaveStatisticsResult();

    }
    private void SaveStatisticsResult()
    {
        // Statistics for ours (comparison of Our method vs. Ground Truth)
        float mpjpeOurs = Util.CalculateMPJPE(modelList[0].GetJointFrameList(), modelList[2].GetJointFrameList());
        float mpjreOurs = Util.CalculateMPJRE(modelList[0].GetJointFrameList(), modelList[2].GetJointFrameList());
        float rooteOurs = Util.CalculateRootE(modelList[0].GetJointFrameList(), modelList[2].GetJointFrameList());

        // Statistics for SMPLify (comparison of SMPLify vs. Ground Truth)
        float mpjpeSmplify = Util.CalculateMPJPE(modelList[1].GetJointFrameList(), modelList[2].GetJointFrameList());
        float mpjreSmplify = Util.CalculateMPJRE(modelList[1].GetJointFrameList(), modelList[2].GetJointFrameList());
        float rooteSmplify = Util.CalculateRootE(modelList[1].GetJointFrameList(), modelList[2].GetJointFrameList());

        // Temporal Joint Jitter calculations
        float tjitterOurs = Util.CalculateTemporalJointJitter(modelList[0].GetJointFrameList());
        float tjitterSmplify = Util.CalculateTemporalJointJitter(modelList[1].GetJointFrameList());
        float tjitterGT = Util.CalculateTemporalJointJitter(modelList[2].GetJointFrameList());

        // Lists for joints comparison
        List<JointData> ourJointList = modelList[0].GetJointFrameList();
        List<JointData> smplifyJointList = modelList[1].GetJointFrameList();
        List<JointData> gtJointList = modelList[2].GetJointFrameList();

        // Writing all statistics to a single file
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
                // Overall statistics for ours
                writer.WriteLine("Statistics for Our Method (compared to GT):");
                writer.WriteLine("MPJPE: " + mpjpeOurs);
                writer.WriteLine("MPJRE: " + mpjreOurs);
                writer.WriteLine("RootE: " + rooteOurs);
                writer.WriteLine("");

                // Overall statistics for SMPLify
                writer.WriteLine("Statistics for SMPLify (compared to GT):");
                writer.WriteLine("MPJPE: " + mpjpeSmplify);
                writer.WriteLine("MPJRE: " + mpjreSmplify);
                writer.WriteLine("RootE: " + rooteSmplify);
                writer.WriteLine("");

                // Temporal Joint Jitter statistics
                writer.WriteLine("Temporal Joint Jitter:");
                writer.WriteLine("Our Method: " + tjitterOurs);
                writer.WriteLine("SMPLify: " + tjitterSmplify);
                writer.WriteLine("Ground Truth: " + tjitterGT);
                writer.WriteLine("");

                // Joint-specific statistics
                writer.WriteLine("Joint-Specific Statistics(compared to GT):");
                for (int i = 0; i < JointData.boneIndexNamesOpenpose.Length; i++)
                {
                    float _mpjpeOur = Util.CalculateMPJPEByJoint(ourJointList, gtJointList, i);
                    float _mpjpeSMPLify = Util.CalculateMPJPEByJoint(smplifyJointList, gtJointList, i);

                    // TODO: Left Elbow -> 0
                    float _mpjreOur = Util.CalculateMPJREByJoint(ourJointList, gtJointList, i);
                    float _mpjreSMPLify = Util.CalculateMPJREByJoint(smplifyJointList, gtJointList, i);

                    float _tjitterOur = Util.CalculateTemporalJointJitterByJoint(ourJointList, i);
                    float _tjitterSMPLify = Util.CalculateTemporalJointJitterByJoint(smplifyJointList, i);
                    float _tjitterGT = Util.CalculateTemporalJointJitterByJoint(gtJointList, i);

                    string jointName = JointData.boneIndexNamesOpenpose[i];

                    writer.WriteLine($"Joint Name: {jointName}");
                    writer.WriteLine($"MPJPE (Our): {_mpjpeOur}");
                    writer.WriteLine($"MPJPE (SMPLify): {_mpjpeSMPLify}");
                    writer.WriteLine($"MPJRE (Our): {_mpjreOur}");
                    writer.WriteLine($"MPJRE (SMPLify): {_mpjreSMPLify}");
                    writer.WriteLine($"Temporal Joint Jitter (Our): {_tjitterOur}");
                    writer.WriteLine($"Temporal Joint Jitter (SMPLify): {_tjitterSMPLify}");
                    writer.WriteLine($"Temporal Joint Jitter (GT): {_tjitterGT}");
                    writer.WriteLine("");  // Add an empty line for better readability
                }
            }

            Console.WriteLine("Statistics saved successfully to " + filepath);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error writing to file: " + e.Message);
        }
    }
}
