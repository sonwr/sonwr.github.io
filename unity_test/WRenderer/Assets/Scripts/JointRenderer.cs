using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using System;

public class JointRenderer : MonoBehaviour
{
    // Prefab for the parent of joints
    public GameObject jointsParentDeepRobot;
    public GameObject jointsParentSMPLify;
    public GameObject jointsParentGroundTruth;

    // Prefab for the sphere to represent joints
    public GameObject jointPrefabDeepRobot;
    public GameObject jointPrefabSMPLify;
    public GameObject jointPrefabGroundTruth;

    // Prefab for the cylinder to represent bones
    public GameObject bonePrefabDeepRobot;
    public GameObject bonePrefabSMPLify;
    public GameObject bonePrefabGroundTruth;

    // Joint
    public List<GameObject> jointGameObjectsDeepRobot;
    public List<GameObject> jointGameObjectsSMPLify;
    public List<GameObject> jointGameObjectsGroundTruth;

    // Bone
    public List<GameObject> boneGameObjectsDeepRobot;
    public List<GameObject> boneGameObjectsSMPLify;
    public List<GameObject> boneGameObjectsGroundTruth;

    // Internal use
    public int frameIndex = 2100;//1800;
    public int frameStartIndex = 2100;//1800;
    public int frameLastIndex = 2350;//3600; //2399;  // 3600;

    private int jointCountDeepRobot = 13;
    private int jointCountSMPLify = 24;
    private int jointCountGroundTruth = 25;

    private float scaleFactorDeepRobot = 0.01f;

    private float alignScaleFactorSMPLify = 1.0f;
    private Vector3 alignTransformSMPLify = new Vector3();

    private float alignScaleFactorGroundTruth = 1.0f;
    private Vector3 alignTransformGroundTruth = new Vector3();

    // The parent index for each joint
    private int[] jointParentIndexDeepRobot = new int[] { -1, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8 };
    //private int[] jointParentIndexSMPLify = new int[] { 3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 12, 12, 24, 13, 14, 16, 17, 18, 19, 20, 21, 24 };
    private int[] jointParentIndexSMPLify = new int[] { 3, 0, 0, 6, 1, 2, 9, 4, 5, 12, 7, 8, 15, 9, 9, -1, 13, 14, 16, 17, 18, 19, 20, 21 };
    private int[] jointParentIndexGroundTruth = new int[] { 1, -1, 1, 2, 3, 1, 5, 6, 1, 8, 9, 10, 8, 12, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    // Ground Truth
    JointDataGroundTruth jointDataGroundTruth;
    ShapeDataGroundTruth shapeDataGroundTruth;
    public PoseDataGroundTruth poseDataGroundTruth;

    // SMPL
    public SMPLBlendshapes smplBlendshapesGroundTruth;

    // Playback
    private float nextActionTime = 0f;
    private float period = 1f / 30f; // 30 FPS에 해당하는 시간 간격


    // Start is called before the first frame update
    void Start()
    {
        InitData();

        string filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_Joints_World3D_opose25_smooth.json";
        jointDataGroundTruth = LoadGroundTruthJointData(filePath);

        filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_SMPL_Shape.json";
        shapeDataGroundTruth = LoadGroundTruthShapeData(filePath);

        filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_SMPL_Pose.json";
        poseDataGroundTruth = LoadGroundTruthPoseData(filePath);        

        DataLoader(true);
        DataLoader(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextActionTime)
        {
            nextActionTime += period;

            frameIndex++;
            if (frameIndex > frameLastIndex)
                frameIndex = frameStartIndex;
            
            DataLoader(false);
        }
    }

    private void InitData()
    {
        // -------------------
        // DeepRobot Joints - Create a list to store the instantiated joint GameObjects
        jointGameObjectsDeepRobot = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < jointCountDeepRobot; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefabDeepRobot, jointPosition, Quaternion.identity);
            joint.transform.SetParent(jointsParentDeepRobot.transform);
            jointGameObjectsDeepRobot.Add(joint);

            GameObject bone = Instantiate(bonePrefabDeepRobot, jointPosition, Quaternion.identity);
            bone.transform.SetParent(jointsParentDeepRobot.transform);
            bone.SetActive(false);
            boneGameObjectsDeepRobot.Add(bone);
        }

        // -------------------
        // SMPLify Joints - Create a list to store the instantiated joint GameObjects
        jointGameObjectsSMPLify = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < jointCountSMPLify; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefabSMPLify, jointPosition, Quaternion.identity);
            joint.transform.SetParent(jointsParentSMPLify.transform);
            jointGameObjectsSMPLify.Add(joint);

            GameObject bone = Instantiate(bonePrefabSMPLify, jointPosition, Quaternion.identity);
            bone.transform.SetParent(jointsParentSMPLify.transform);
            bone.SetActive(false);
            boneGameObjectsSMPLify.Add(bone);
        }

        // -------------------
        // Ground Truth Joints - Create a list to store the instantiated joint GameObjects
        jointGameObjectsGroundTruth = new List<GameObject>();

        // Instantiate joints and store them in the list
        for (int i = 0; i < jointCountGroundTruth; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefabGroundTruth, jointPosition, Quaternion.identity);
            joint.transform.SetParent(jointsParentGroundTruth.transform);
            jointGameObjectsGroundTruth.Add(joint);

            GameObject bone = Instantiate(bonePrefabGroundTruth, jointPosition, Quaternion.identity);
            bone.transform.SetParent(jointsParentGroundTruth.transform);
            bone.SetActive(false);
            boneGameObjectsGroundTruth.Add(bone);
        }
    }

    private void DataLoader(bool calcAlign)
    {
        List<Vector3> jointsListDeepRobot = new List<Vector3>();
        List<Vector3> jointsListSMPLify = new List<Vector3>();
        List<Vector3> jointsListGroundTruth = new List<Vector3>();

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
                //jointGameObjectsDeepRobot[i].transform.position = position * scaleFactorDeepRobot;
                jointGameObjectsDeepRobot[i].transform.localPosition = position * scaleFactorDeepRobot;
            }

            // Bone position
            for (int i = 0; i < joints.Length; i++)
            {
                if (jointParentIndexDeepRobot[i] < 0)
                    continue;

                GameObject childJoint = jointGameObjectsDeepRobot[i];
                GameObject parentJoint = jointGameObjectsDeepRobot[jointParentIndexDeepRobot[i]];

                Vector3 direction = childJoint.transform.position - parentJoint.transform.position;
                float distance = direction.magnitude;
                direction.Normalize();

                boneGameObjectsDeepRobot[i].transform.position = parentJoint.transform.position + direction * distance * 0.5f;
                boneGameObjectsDeepRobot[i].transform.up = direction;
                boneGameObjectsDeepRobot[i].transform.localScale = new Vector3(0.05f, distance * 0.5f, 0.05f);
                boneGameObjectsDeepRobot[i].SetActive(true);
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
                Vector3 newPosition = (position * alignScaleFactorSMPLify) + alignTransformSMPLify;

                // GameObject
                jointGameObjectsSMPLify[i].transform.localPosition = newPosition * scaleFactorDeepRobot;
            }

            // Bone position
            for (int i = 0; i < joints.Length; i++)
            {
                if (jointParentIndexSMPLify[i] < 0)
                    continue;

                GameObject childJoint = jointGameObjectsSMPLify[i];
                GameObject parentJoint = jointGameObjectsSMPLify[jointParentIndexSMPLify[i]];

                Vector3 direction = childJoint.transform.position - parentJoint.transform.position;
                float distance = direction.magnitude;
                direction.Normalize();

                boneGameObjectsSMPLify[i].transform.position = parentJoint.transform.position + direction * distance * 0.5f;
                boneGameObjectsSMPLify[i].transform.up = direction;
                boneGameObjectsSMPLify[i].transform.localScale = new Vector3(0.05f, distance * 0.5f, 0.05f);
                boneGameObjectsSMPLify[i].SetActive(true);
            }
        }

        // Ground Truth Joints
        if (jointGameObjectsGroundTruth.Count >= jointCountGroundTruth)
        {
            JointDataFrame frameData = jointDataGroundTruth.Set[frameIndex - frameStartIndex];
            var joints = frameData.J;

            // Instantiate joints and store them in the list
            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 position = new Vector3(joints[i][0], -joints[i][1], joints[i][2]);
                jointsListGroundTruth.Add(position);

                // Alignment
                Vector3 newPosition = (position * alignScaleFactorGroundTruth) + alignTransformGroundTruth;

                // GameObject
                jointGameObjectsGroundTruth[i].transform.localPosition = newPosition * scaleFactorDeepRobot;
            }

            // Bone position
            for (int i = 0; i < joints.Length; i++)
            {
                if (jointParentIndexGroundTruth[i] < 0)
                    continue;

                GameObject childJoint = jointGameObjectsGroundTruth[i];
                GameObject parentJoint = jointGameObjectsGroundTruth[jointParentIndexGroundTruth[i]];

                Vector3 direction = childJoint.transform.position - parentJoint.transform.position;
                float distance = direction.magnitude;
                direction.Normalize();

                boneGameObjectsGroundTruth[i].transform.position = parentJoint.transform.position + direction * distance * 0.5f;
                boneGameObjectsGroundTruth[i].transform.up = direction;
                boneGameObjectsGroundTruth[i].transform.localScale = new Vector3(0.05f, distance * 0.5f, 0.05f);
                boneGameObjectsGroundTruth[i].SetActive(true);
            }

            // SMPL Shape & Pose Parameters
            ShapeDataFrame shapeDataFrame = shapeDataGroundTruth.shape_param_frames[frameIndex - frameStartIndex];
            PoseDataFrame poseDataFrame = poseDataGroundTruth.pose_parameters[frameIndex - frameStartIndex];
            smplBlendshapesGroundTruth.setShapeParms(shapeDataFrame.S, poseDataFrame.R);
        }

        // Calculate Alignment
        if (calcAlign == true)
        {
            // DeepRobot <-> SMPLify
            //(alignScaleFactorSMPLify, alignTransformSMPLify) = AdjustScaleAndPosition(jointsListDeepRobot, jointsListSMPLify);
            (alignScaleFactorSMPLify, alignTransformSMPLify) = AdjustScaleAndPosition(jointsListDeepRobot[1], jointsListDeepRobot[2], jointsListSMPLify[16], jointsListSMPLify[17]);

            // DeepRobot <-> GroundTruth
            (alignScaleFactorGroundTruth, alignTransformGroundTruth) = AdjustScaleAndPosition(jointsListDeepRobot[1], jointsListDeepRobot[2], jointsListGroundTruth[2], jointsListGroundTruth[5]);
        }
    }

    private JointDataGroundTruth LoadGroundTruthJointData(string filePath)
    {
        try
        {
            string jsonData = File.ReadAllText(filePath);
            JointDataGroundTruth poseData = JsonConvert.DeserializeObject<JointDataGroundTruth>(jsonData);
            return poseData;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error reading joint data: " + ex.Message);
            return null;
        }
    }

    private ShapeDataGroundTruth LoadGroundTruthShapeData(string filePath)
    {
        try
        {
            string jsonData = File.ReadAllText(filePath);
            ShapeDataGroundTruth shapeData = JsonConvert.DeserializeObject<ShapeDataGroundTruth>(jsonData);
            return shapeData;

        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error reading shape data: " + ex.Message);
            return null;
        }
    }

    private PoseDataGroundTruth LoadGroundTruthPoseData(string filePath)
    {
        try
        {
            string jsonData = File.ReadAllText(filePath);
            PoseDataGroundTruth poseData = JsonConvert.DeserializeObject<PoseDataGroundTruth>(jsonData);
            return poseData;

        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError("Error reading pose data: " + ex.Message);
            return null;
        }
    }

    private (float, Vector3) AdjustScaleAndPosition(List<Vector3> jointsListDeepRobot, List<Vector3> jointsListSMPLify)
    {
        // 스케일 계산
        float robotDistance = Vector3.Distance(jointsListDeepRobot[1], jointsListDeepRobot[2]);
        float smplifyDistance = Vector3.Distance(jointsListSMPLify[16], jointsListSMPLify[17]);
        float scale = robotDistance / smplifyDistance;

        // 위치 조정: 어깨 중심점을 기준으로 조정
        Vector3 robotShoulderCenter = (jointsListDeepRobot[1] + jointsListDeepRobot[2]) / 2;
        Vector3 smplifyShoulderCenter = (jointsListSMPLify[16] + jointsListSMPLify[17]) / 2 * scale;
        Vector3 displacement = robotShoulderCenter - smplifyShoulderCenter;

        // 위치 조정
        //Vector3 robotPoint = jointsListDeepRobot[(int)JOINT_IDX_3D.CAPSKEL_Neck];
        //Vector3 smplifyPoint = jointsListSMPLify[12] * scale;
        //Vector3 displacement = robotPoint - smplifyPoint;

        return (scale, displacement);
    }
    

    private (float, Vector3) AdjustScaleAndPosition(Vector3 LShoulderDeepRobot, Vector3 RShoulderDeepRobot, Vector3 LShoulderSMPLify, Vector3 RShoulderSMPLify)
    {
        // 스케일 계산
        float robotDistance = Vector3.Distance(LShoulderDeepRobot, RShoulderDeepRobot);
        float smplifyDistance = Vector3.Distance(LShoulderSMPLify, RShoulderSMPLify);
        float scale = robotDistance / smplifyDistance;

        // 위치 조정: 어깨 중심점을 기준으로 조정
        Vector3 robotShoulderCenter = (LShoulderDeepRobot + RShoulderDeepRobot) / 2;
        Vector3 smplifyShoulderCenter = (LShoulderSMPLify + RShoulderSMPLify) / 2 * scale;
        Vector3 displacement = robotShoulderCenter - smplifyShoulderCenter;

        return (scale, displacement);
    }
}
