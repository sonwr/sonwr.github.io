using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;

public enum BodyType
{
    None = 0,
    DeepRobot = 1,
    SMPLify = 2,
    GroundTruth = 3,
}

public class BodyData : MonoBehaviour
{
    // SMPLify
    private string joint_filename_smplify = Directory.GetCurrentDirectory() + "/Data/2024-05-24/Seq1/SMPlify/pos_smplify.json";

    // GT
    private string joint_filename_gt = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/Frameset_Joints_World3D_opose25_smooth.json";
    private string pose_filename_gt = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/Frameset_SMPL_Pose.json";
    private string shape_filename_gt = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/Frameset_SMPL_Shape.json";

    // Ours
    private string joint_filename_ours = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/Frameset_Joints_World3D_opose25_Stereo_Ours.json";
    private string pose_filename_ours = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/Frameset_SMPL_Pose_Stereo_Ours.json";
    private string shape_filename_ours = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/Frameset_SMPL_Shape_Stereo_Ours.json";
    
    /*
    private string joint_filename_gt = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq2/Frameset_Joints_World3D_opose25_smooth.json";
    private string pose_filename_gt = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq2/Frameset_SMPL_Pose.json";
    private string shape_filename_gt = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq2/Frameset_SMPL_Shape.json";

    private string joint_filename_ours = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq2/Frameset_Joints_World3D_opose25_Stereo_Ours.json";
    private string pose_filename_ours = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq2/Frameset_SMPL_Pose_Stereo_Ours.json";
    private string shape_filename_ours = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq2/Frameset_SMPL_Shape_Stereo_Ours.json";
    */



    private BodyType modelType = BodyType.None;
    private Color color = Color.white;

    private static string[] modelNames = new string[] { "DeepRobot", "SMPLify", "GroundTruth" };

    private List<JointData> jointFrameList;

    // Game Object
    private List<GameObject> jointGameObjects;
    private List<GameObject> boneGameObjects;
    private GameObject smplObject;
    private GameObject smplObject2;

    private float alignScale = 1.0f;
    private Vector3 alignDisplacement = new Vector3();

    public BodyData(BodyType modelType)
	{
        this.modelType = modelType;

        if (modelType == BodyType.DeepRobot)
            color = Color.blue;
        if (modelType == BodyType.SMPLify)
            color = Color.red;
        if (modelType == BodyType.GroundTruth)
            color = Color.yellow;
    }

    public List<JointData> GetJointFrameList()
    {
        return jointFrameList;
    }

    public string GetModelName()
    {
        if (modelType == BodyType.DeepRobot)
            return modelNames[0];
        if (modelType == BodyType.SMPLify)
            return modelNames[1];
        if (modelType == BodyType.GroundTruth)
            return modelNames[2];

        return null;
    }

    public void Init(GameObject jointPrefab, GameObject bonePrefab, GameObject parentGameObject)
    {
        jointGameObjects = new List<GameObject>();
        boneGameObjects = new List<GameObject>();

        // Init rotation
        Quaternion rotation = Quaternion.identity;
        /*
        if (modelType == BodyType.DeepRobot)
            rotation = Quaternion.Euler(0, 0, 180);
        else if (modelType == BodyType.SMPLify)
            rotation = Quaternion.Euler(0, 0, 180);
        else if (modelType == BodyType.GroundTruth)
            rotation = Quaternion.identity;
        */

        parentGameObject.transform.rotation = rotation;

        // Instantiate joints and store them in the list
        for (int i = 0; i < JointData.jointCount; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.GetComponent<Renderer>().material.color = color;
            joint.transform.SetParent(parentGameObject.transform);
            joint.name = joint.name + "_" + i.ToString();
            jointGameObjects.Add(joint);

            GameObject bone = Instantiate(bonePrefab, jointPosition, Quaternion.identity);
            bone.transform.SetParent(parentGameObject.transform);
            bone.GetComponent<Renderer>().material.color = color;
            bone.SetActive(false);
            boneGameObjects.Add(bone);
        }
    }

    public void InitSMPL(GameObject smplPrefab, GameObject parentGameObject)
    {
        // Init position
        Vector3 position = new Vector3(0, 0, 0);
        /*
        if (modelType == BodyType.DeepRobot)
            position = new Vector3(0.0f, 0, 0);
        else if (modelType == BodyType.SMPLify)
            position = new Vector3(-1.0f, 0, 0);
        else if (modelType == BodyType.GroundTruth)
            position = new Vector3(1.0f, 0, 0);
        */

        // Init rotation
        Quaternion rotation = Quaternion.identity;
        if (modelType == BodyType.DeepRobot)
            rotation = Quaternion.identity; // rotation = Quaternion.Euler(0, 180, 0);
        else if (modelType == BodyType.SMPLify)
            rotation = Quaternion.Euler(0, 0, 180);
        else if (modelType == BodyType.GroundTruth)
            rotation = Quaternion.identity;

        // SMPL 1
        {
            smplObject = Instantiate(smplPrefab, position, rotation);
            Transform childTransform = smplObject.transform.Find("m_avg");
            childTransform.GetComponent<Renderer>().material.color = color;
            smplObject.transform.SetParent(parentGameObject.transform);
        }

        // SMPL 2
        {
            position = new Vector3(2.0f, 0, 0);

            smplObject2 = Instantiate(smplPrefab, position, rotation);
            Transform childTransform = smplObject2.transform.Find("m_avg");
            childTransform.GetComponent<Renderer>().material.color = color;
            smplObject2.transform.SetParent(parentGameObject.transform);
        }
    }

    public void Render(int frameIndex, int frameStartIndex, int frameLastIndex)
    {
        if (modelType == BodyType.SMPLify)
        {
            smplObject.transform.Rotate(0, 1, 0);
            smplObject2.transform.Rotate(0, 1, 0);
        }
        else
        {
            smplObject.transform.Rotate(0, -1, 0);
            smplObject2.transform.Rotate(0, -1, 0);
        }


        int idx = frameIndex - frameStartIndex;
        if (jointFrameList == null || jointFrameList.Count <= idx)
            return;

        // Joint
        JointData jointData = jointFrameList[idx];

        for(int i=0; i<jointData.jointList.Count; i++)
        {
            Vector3 joint = (jointData.jointList[i] * alignScale) + alignDisplacement;
            jointGameObjects[i].transform.localPosition = joint;

            if (jointData.jointList[i] == Vector3.zero)
                jointGameObjects[i].SetActive(false);
            else
                jointGameObjects[i].SetActive(true);
        }
        
        // Bone
        for (int i = 0; i < jointData.jointList.Count; i++)
        {
            if (JointData.jointParentIndexOpenpose[i] < 0)
                continue;

            GameObject childJoint = jointGameObjects[i];
            GameObject parentJoint = FindActiveParent(i);

            if (childJoint.activeSelf == false || parentJoint.activeSelf == false || parentJoint == null)
                continue;

            Vector3 direction = childJoint.transform.position - parentJoint.transform.position;
            float distance = direction.magnitude;
            direction.Normalize();

            boneGameObjects[i].transform.position = parentJoint.transform.position + direction * distance * 0.5f;
            boneGameObjects[i].transform.up = direction;
            boneGameObjects[i].transform.localScale = new Vector3(0.05f, distance * 0.5f, 0.05f);
            boneGameObjects[i].SetActive(true);
        }

        // SMPL 1
        {
            Transform childTransform = smplObject.transform.Find("m_avg");
            SMPLBlendshapes smplBlendshapes = childTransform.GetComponent<SMPLBlendshapes>();

            if (jointData.shapeList.Count == JointData.shapeCount && jointData.poseList.Count == JointData.poseCount)
                smplBlendshapes.setShapeAndPoseParameters(jointData.shapeList, jointData.poseList, new Vector3(0, 0, 0));
        }

        // SMPL 2
        {
            JointData firstFrameJointData = jointFrameList[0];
            Vector3 pelvisPosition1 = firstFrameJointData.CalculatePelvisPosition();
            Vector3 pelvisPosition2 = jointData.CalculatePelvisPosition();
            Vector3 pelvisPosition = pelvisPosition2 - pelvisPosition1;

            Transform childTransform = smplObject2.transform.Find("m_avg");
            SMPLBlendshapes smplBlendshapes = childTransform.GetComponent<SMPLBlendshapes>();

            if (jointData.shapeList.Count == JointData.shapeCount && jointData.poseList.Count == JointData.poseCount)
                smplBlendshapes.setShapeAndPoseParameters(jointData.shapeList, jointData.poseList, pelvisPosition);
        }
    }

    // 활성 상태인 부모 GameObject를 재귀적으로 찾는 함수
    private GameObject FindActiveParent(int currentIndex)
    {
        if (currentIndex < 0)
            return null;

        int parentIndex = JointData.jointParentIndexOpenpose[currentIndex];
        if (jointGameObjects[parentIndex].activeSelf)
            return jointGameObjects[parentIndex];
        else
            return FindActiveParent(parentIndex);
    }

    public void LoadFile(int frameStartIndex, int frameLastIndex)
    {
        jointFrameList = new List<JointData>();

        //if (modelType == BodyType.DeepRobot || modelType == BodyType.SMPLify)
        //    LoadFileDeepRobotOrSMPLify(frameStartIndex, frameLastIndex);

        //if (modelType == BodyType.SMPLify)
        //    LoadFileDeepRobotOrSMPLify(frameStartIndex, frameLastIndex);


        if (modelType == BodyType.SMPLify)
            LoadFileSMPLify(frameStartIndex, frameLastIndex);

        if (modelType == BodyType.GroundTruth)
            LoadFileGroundTruth(frameStartIndex, frameLastIndex);

        if (modelType == BodyType.DeepRobot)
            LoadFileDeepRobot(frameStartIndex, frameLastIndex);
    }


    private void LoadFileSMPLify(int frameStartIndex, int frameLastIndex)
    {
        // Joint
        string filePath = joint_filename_smplify;
        string jsonData = File.ReadAllText(filePath);
        JointDataGroundTruth jointDataGroundTruth = JsonConvert.DeserializeObject<JointDataGroundTruth>(jsonData);

        string path = Directory.GetCurrentDirectory() + "/Data/2024-05-24/Seq1/SMPLify/";

        for (int i = 0; i < jointDataGroundTruth.Set.Count; i++)
        {
            // Position
            JointDataFrame jointDataFrame = jointDataGroundTruth.Set[i];
            int frameIndex = jointDataFrame.F;

            if (frameIndex < frameStartIndex || frameIndex >= frameLastIndex)
                continue;

            JointData jointData = new JointData(frameIndex);

            float scale = 1.0f;// 0.01f;

            List<Vector3> jointList = new List<Vector3>();
            for (int j = 0; j < jointDataFrame.J.Length; j++)
            {
                float x = jointDataFrame.J[j][0] * scale;
                float y = jointDataFrame.J[j][1] * scale;
                float z = jointDataFrame.J[j][2] * scale;

                Vector3 position = new Vector3(x, y, z);
                jointList.Add(position);
            }

            jointData.jointList = jointList;



            // SMPLify
            // Read and process the JSON file
            filePath = path + "param_smplify_" + i.ToString() + ".json";
            if (filePath == null)
                continue;

            string dataAsJson = File.ReadAllText(filePath);
            PoseAndShapeDataSMPLify poseAndShapeData = JsonConvert.DeserializeObject<PoseAndShapeDataSMPLify>(dataAsJson);

            // Converting pose parameters to Quaternion
            List<Quaternion> poseList = new List<Quaternion>();

            for (int k = 0; k < poseAndShapeData.pose.Count; k += 3)
            {
                Vector3 rodVector = new Vector3(poseAndShapeData.pose[k], poseAndShapeData.pose[k + 1], poseAndShapeData.pose[k + 2]);
                Quaternion quaternion = Util.RodriguesToQuaternion(rodVector);
                poseList.Add(quaternion);
            }
            jointData.poseList = Util.ConvertOpenGLToUnity(poseList);

            // Assigning shape parameters directly
            jointData.shapeList = poseAndShapeData.betas;
            


            jointFrameList.Add(jointData);
        }
    }

    private void LoadFileDeepRobot(int frameStartIndex, int frameLastIndex)
    {
        // Joint
        string filePath = joint_filename_ours;
        string jsonData = File.ReadAllText(filePath);
        JointDataGroundTruth jointDataGroundTruth = JsonConvert.DeserializeObject<JointDataGroundTruth>(jsonData);

        // Shape
        filePath = shape_filename_ours;
        jsonData = File.ReadAllText(filePath);
        ShapeDataGroundTruth shapeData = JsonConvert.DeserializeObject<ShapeDataGroundTruth>(jsonData);

        // Pose
        filePath = pose_filename_ours;
        jsonData = File.ReadAllText(filePath);
        PoseDataGroundTruth poseData = JsonConvert.DeserializeObject<PoseDataGroundTruth>(jsonData);

        for (int i = 0; i < jointDataGroundTruth.Set.Count; i++)
        {
            JointDataFrame jointDataFrame = jointDataGroundTruth.Set[i];
            int frameIndex = jointDataFrame.F;

            if (frameIndex < frameStartIndex || frameIndex >= frameLastIndex)
                continue;

            JointData jointData = new JointData(frameIndex);

            float scale = 0.01f;

            List<Vector3> jointList = new List<Vector3>();
            for (int j = 0; j < jointDataFrame.J.Length; j++)
            {
                float x = jointDataFrame.J[j][0] * scale;
                float y = jointDataFrame.J[j][1] * scale;
                float z = jointDataFrame.J[j][2] * scale;

                Vector3 position = new Vector3(x, y, z);
                jointList.Add(position);
            }

            jointData.jointList = jointList;
            jointData.shapeList = shapeData.shape_param_frames[i].S;

            List<Quaternion> poseList = Util.ConvertOpenGLToUnity(Util.ConvertToQuaternions(poseData.pose_parameters[i].R));
            jointData.poseList = Util.ConvertGroundTruthRotationToSMPL(poseList);

            jointFrameList.Add(jointData);
        }
    }

    private void LoadFileGroundTruth(int frameStartIndex, int frameLastIndex)
    {
        // Joint
        //string filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_Joints_World3D_opose25_smooth.json";
        string filePath = joint_filename_gt;
        string jsonData = File.ReadAllText(filePath);
        JointDataGroundTruth jointDataGroundTruth = JsonConvert.DeserializeObject<JointDataGroundTruth>(jsonData);

        // Shape
        //filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_SMPL_Shape.json";
        filePath = shape_filename_gt;
        jsonData = File.ReadAllText(filePath);
        ShapeDataGroundTruth shapeData = JsonConvert.DeserializeObject<ShapeDataGroundTruth>(jsonData);

        // Pose
        //filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_SMPL_Pose.json";
        filePath = pose_filename_gt;
        jsonData = File.ReadAllText(filePath);
        PoseDataGroundTruth poseData = JsonConvert.DeserializeObject<PoseDataGroundTruth>(jsonData);

        for (int i=0; i< jointDataGroundTruth.Set.Count; i++)
        {
            JointDataFrame jointDataFrame = jointDataGroundTruth.Set[i];
            int frameIndex = jointDataFrame.F;

            if (frameIndex < frameStartIndex || frameIndex >= frameLastIndex)
                continue;

            JointData jointData = new JointData(frameIndex);

            float scale = 0.01f;

            List<Vector3> jointList = new List<Vector3>();
            for (int j = 0; j < jointDataFrame.J.Length; j++)
            {
                float x = jointDataFrame.J[j][0] * scale;
                float y = jointDataFrame.J[j][1] * scale;
                float z = jointDataFrame.J[j][2] * scale;

                Vector3 position = new Vector3(x, y, z);
                jointList.Add(position);
            }

            jointData.jointList = jointList;
            jointData.shapeList = shapeData.shape_param_frames[i].S;

            List<Quaternion> poseList = Util.ConvertOpenGLToUnity(Util.ConvertToQuaternions(poseData.pose_parameters[i].R));
            jointData.poseList = Util.ConvertGroundTruthRotationToSMPL(poseList);

            jointFrameList.Add(jointData);
        }
    }

    private void LoadFileDeepRobotOrSMPLify(int frameStartIndex, int frameLastIndex)
    {
        string path = Directory.GetCurrentDirectory() + "/Data/2024-05-07/Seq1/SMPLify/";

        for (int i = frameStartIndex; i < frameLastIndex; i++)
        {
            JointData jointData = new JointData(i);

            // Read and process the JSON file
            string filePath = null;

            if (modelType == BodyType.DeepRobot)
                filePath = Directory.GetCurrentDirectory() + "/Data/" + i.ToString() + "_3_joint_3d.json";
            else if (modelType == BodyType.SMPLify)
                filePath = Directory.GetCurrentDirectory() + "/Data/f_" + i.ToString() + "_3_joint_3d_smplify4.json";
                //filePath = path + "USB_Sync_Left_" + i.ToString() + ""

            if (filePath == null)
                continue;

            string dataAsJson = File.ReadAllText(filePath);
            var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

            // Scale
            float scale = 1.0f;
            if (modelType == BodyType.DeepRobot)
                scale = 0.01f;

            // Instantiate joints and store them in the list
            List<Vector3> jointList = new List<Vector3>();
            for (int j = 0; j < joints.Length; j++)
            {
                float x = joints[j][0] * scale;
                float y = joints[j][1] * scale * -1;
                float z = joints[j][2] * scale;

                Vector3 position = new Vector3(x, y, z);
                jointList.Add(position);
            }

            // Convert
            if (modelType == BodyType.DeepRobot)
                jointData.jointList = Util.ConvertJointDataOrder(jointList, JointData.boneIndexNamesDeepRobotJoint, JointData.boneIndexNamesOpenpose);
            else if (modelType == BodyType.SMPLify)
                jointData.jointList = Util.ConvertJointDataOrder(jointList, JointData.boneIndexNamesSMPL, JointData.boneIndexNamesOpenpose);


            // Shape & Pose
            if (modelType == BodyType.DeepRobot)
            {
                // Shape
                jointData.shapeList = new List<float>();
                for (int j = 0; j < JointData.shapeCount; j++)
                    jointData.shapeList.Add(0.0f);

                // Pose
                jointData.poseList = new List<Quaternion>();
                for (int j = 0; j < JointData.poseCount; j++)
                    jointData.poseList.Add(new Quaternion());

                GenerateDeepRobotPoseData(jointData.poseList, jointData.jointList);
            }
            else if (modelType == BodyType.SMPLify)
            {
                filePath = Directory.GetCurrentDirectory() + "/Data/f_" + i.ToString() + "_4_output_smplify.json";
                dataAsJson = File.ReadAllText(filePath);
                PoseAndShapeDataSMPLify poseAndShapeData = JsonConvert.DeserializeObject<PoseAndShapeDataSMPLify>(dataAsJson);

                // Converting pose parameters to Quaternion
                List<Quaternion> poseList = new List<Quaternion>();

                for (int k = 0; k < poseAndShapeData.pose.Count; k += 3)
                {
                    Vector3 rodVector = new Vector3(poseAndShapeData.pose[k], poseAndShapeData.pose[k + 1], poseAndShapeData.pose[k + 2]);
                    Quaternion quaternion = Util.RodriguesToQuaternion(rodVector);
                    poseList.Add(quaternion);
                }
                jointData.poseList = Util.ConvertOpenGLToUnity(poseList);

                // Assigning shape parameters directly
                jointData.shapeList = poseAndShapeData.betas;
            }

            jointFrameList.Add(jointData);
        }
    }

    private void GenerateDeepRobotPoseData(List<Quaternion> poseList, List<Vector3> jointList)
    {
        if (poseList == null || poseList.Count < JointData.poseCount)
            return;

        List<Vector3> convertedJointList = Util.MultiplyYByNegativeOne(jointList);

        //poseList[0] = Quaternion.Euler(0, 180, 0);

        // Left Shoulder
        poseList[17] = Util.CalcJointRotation(convertedJointList[1], convertedJointList[5], convertedJointList[6]);
        // Left Elbow
        poseList[19] = Util.CalcJointRotation(convertedJointList[5], convertedJointList[6], convertedJointList[7]);

        // Right Shoulder
        poseList[16] = Util.CalcJointRotation(convertedJointList[1], convertedJointList[2], convertedJointList[3]);
        // Right Elbow
        poseList[18] = Util.CalcJointRotation(convertedJointList[2], convertedJointList[3], convertedJointList[4]);

        // TODO: 좌우 다리가 반대로 나옴

        // Left Hip
        poseList[1] = Util.CalcJointRotation(convertedJointList[1], convertedJointList[12], convertedJointList[13]);
        // Left Knee
        poseList[4] = Util.CalcJointRotation(convertedJointList[12], convertedJointList[13], convertedJointList[14]);

        // Right Hip
        poseList[2] = Util.CalcJointRotation(convertedJointList[1], convertedJointList[9], convertedJointList[10]);
        // Right Knee
        poseList[5] = Util.CalcJointRotation(convertedJointList[9], convertedJointList[10], convertedJointList[11]);
    }

    public void SetScaleAndDisplacement(float scale, Vector3 displacement)
    {
        alignScale = scale;
        alignDisplacement = displacement;
    }

    // src body를 target body에 맞추기 위한 scale과 displacement(tranform) 값 구하기
    public static (float, Vector3) AdjustScaleAndPosition(BodyData target, BodyData src)
    {
        // first frame
        Vector3 LShoulder_Target = target.jointFrameList[0].jointList[5];
        Vector3 RShoulder_Target = target.jointFrameList[0].jointList[2];

        Vector3 LShoulder_Src = src.jointFrameList[0].jointList[5];
        Vector3 RShoulder_Src = src.jointFrameList[0].jointList[2];

        // 스케일 계산
        float robotDistance = Vector3.Distance(LShoulder_Target, RShoulder_Target);
        float smplifyDistance = Vector3.Distance(LShoulder_Src, RShoulder_Src);
        float scale = robotDistance / smplifyDistance;

        // 위치 조정: 어깨 중심점을 기준으로 조정
        Vector3 robotShoulderCenter = (LShoulder_Target + RShoulder_Target) / 2;
        Vector3 smplifyShoulderCenter = (LShoulder_Src + RShoulder_Src) / 2 * scale;
        Vector3 displacement = robotShoulderCenter - smplifyShoulderCenter;

        return (scale, displacement);
    }
}

