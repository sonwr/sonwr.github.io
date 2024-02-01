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
    private BodyType modelType = BodyType.None;
    private Color color = Color.white;

    private static string[] modelNames = new string[] { "DeepRobot", "SMPLify", "GroundTruth" };

    private List<JointData> jointFrameList;

    // Game Object
    private List<GameObject> jointGameObjects;
    private List<GameObject> boneGameObjects;
    private GameObject smplObject;

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
        if (modelType == BodyType.DeepRobot)
            rotation = Quaternion.Euler(0, 0, 180);
        else if (modelType == BodyType.SMPLify)
            rotation = Quaternion.Euler(0, 0, 180);
        else if (modelType == BodyType.GroundTruth)
            rotation = Quaternion.identity;

        parentGameObject.transform.rotation = rotation;

        // Instantiate joints and store them in the list
        for (int i = 0; i < JointData.jointCount; i++)
        {
            Vector3 jointPosition = new Vector3(0, 0, 0);
            GameObject joint = Instantiate(jointPrefab, jointPosition, Quaternion.identity);
            joint.GetComponent<Renderer>().material.color = color;
            joint.transform.SetParent(parentGameObject.transform);
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
            rotation = Quaternion.Euler(0, 180, 0);
        else if (modelType == BodyType.SMPLify)
            rotation = Quaternion.Euler(0, 0, 180);
        else if (modelType == BodyType.GroundTruth)
            rotation = Quaternion.identity;

        smplObject = Instantiate(smplPrefab, position, rotation);
        Transform childTransform = smplObject.transform.Find("m_avg");
        childTransform.GetComponent<Renderer>().material.color = color;
        smplObject.transform.SetParent(parentGameObject.transform);
    }

    public void Render(int frameIndex, int frameStartIndex, int frameLastIndex)
    {
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

        // SMPL
        Transform childTransform = smplObject.transform.Find("m_avg");
        SMPLBlendshapes smplBlendshapes = childTransform.GetComponent<SMPLBlendshapes>();

        if (jointData.shapeList.Count == JointData.shapeCount && jointData.poseList.Count == JointData.poseCount)
            smplBlendshapes.setShapeAndPoseParameters(jointData.shapeList, jointData.poseList);
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

        if (modelType == BodyType.DeepRobot || modelType == BodyType.SMPLify)
            LoadFileDeepRobotOrSMPLify(frameStartIndex, frameLastIndex);

        if (modelType == BodyType.GroundTruth)
            LoadFileGroundTruth(frameStartIndex, frameLastIndex);
    }

    private void LoadFileGroundTruth(int frameStartIndex, int frameLastIndex)
    {
        // Joint
        string filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_Joints_World3D_opose25_smooth.json";
        string jsonData = File.ReadAllText(filePath);
        JointDataGroundTruth jointDataGroundTruth = JsonConvert.DeserializeObject<JointDataGroundTruth>(jsonData);

        // Shape
        filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_SMPL_Shape.json";
        jsonData = File.ReadAllText(filePath);
        ShapeDataGroundTruth shapeData = JsonConvert.DeserializeObject<ShapeDataGroundTruth>(jsonData);

        // Pose
        filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_SMPL_Pose.json";
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

            List<Quaternion> poseList = ConvertOpenGLToUnity(ConvertToQuaternions(poseData.pose_parameters[i].R));
            jointData.poseList = ConvertGroundTruthRotationToSMPL(poseList);

            jointFrameList.Add(jointData);
        }
    }

    public List<Quaternion> ConvertGroundTruthRotationToSMPL(List<Quaternion> rotationParameters)
    {
        // Convert global rotations to local rotations
        List<Quaternion> localRotations = ConvertLocalRotation(rotationParameters);

        // Reorder
        List<Quaternion> reorderedRotations = ReorderQuaternions(localRotations, JointData.boneIndexNamesGroundTruth, JointData.boneIndexNamesSMPL);

        return reorderedRotations;
    }

    private List<Quaternion> ConvertLocalRotation(List<Quaternion> globalRotations)
    {
        int[] boneParents = new int[] { -1, 0, 1, 2, 3, 0, 5, 6, 7, 0, 9, 10, 11, 12, 11, 14, 15, 16, 17, 11, 19, 20, 21, 22 };
        List<Quaternion> localRotations = new List<Quaternion>(new Quaternion[globalRotations.Count]);

        for (int i = 0; i < globalRotations.Count; i++)
        {
            int parentIndex = boneParents[i];
            if (parentIndex == -1)
                localRotations[i] = globalRotations[i];
            else
                localRotations[i] = Quaternion.Inverse(globalRotations[parentIndex]) * globalRotations[i];
        }

        return localRotations;
    }

    private List<Quaternion> ReorderQuaternions(List<Quaternion> originalRotations, string[] srcOrder, string[] destOrder)
    {
        List<Quaternion> reorderedRotations = new List<Quaternion>(new Quaternion[destOrder.Length]);

        for (int i = 0; i < destOrder.Length; i++)
        {
            int indexInGroundTruth = System.Array.IndexOf(srcOrder, destOrder[i]);
            if (indexInGroundTruth != -1)
            {
                reorderedRotations[i] = originalRotations[indexInGroundTruth];
            }
            else
            {
                Debug.LogError($"Bone {destOrder[i]} not found in ground truth names.");
                reorderedRotations[i] = Quaternion.identity; // Default rotation if not found
            }
        }

        return reorderedRotations;
    }

    // OpenGL coord -> Unity coord: y flip, z flip
    public static List<Quaternion> ConvertOpenGLToUnity(List<Quaternion> quaternions)
    {
        List<Quaternion> convertedQuaternions = new List<Quaternion>();

        foreach (Quaternion quaternion in quaternions)
        {
            // OpenGL에서 Unity로 변환하기 위해 y와 z의 부호를 변경합니다.
            Quaternion convertedQuaternion = new Quaternion(quaternion.x, -quaternion.y, -quaternion.z, quaternion.w);
            convertedQuaternions.Add(convertedQuaternion);
        }

        return convertedQuaternions;
    }

    public static List<Quaternion> ConvertToQuaternions(List<List<float>> floatLists)
    {
        List<Quaternion> quaternions = new List<Quaternion>();

        foreach (List<float> floatList in floatLists)
        {
            // 정상적인 Quaternion 데이터를 가지고 있는지 확인합니다.
            if (floatList.Count == 4)
            {
                Quaternion quaternion = new Quaternion(floatList[0], floatList[1], floatList[2], floatList[3]);
                quaternions.Add(quaternion);
            }
            else
            {
                Debug.LogError("List<float> does not contain exactly 4 elements.");
            }
        }

        return quaternions;
    }

    private void LoadFileDeepRobotOrSMPLify(int frameStartIndex, int frameLastIndex)
    {
        for (int i = frameStartIndex; i < frameLastIndex; i++)
        {
            JointData jointData = new JointData(i);

            // Read and process the JSON file
            string filePath = null;

            if (modelType == BodyType.DeepRobot)
                filePath = Directory.GetCurrentDirectory() + "/Data/f_" + i.ToString() + "_3_joint_3d.json";
            else if (modelType == BodyType.SMPLify)
                filePath = Directory.GetCurrentDirectory() + "/Data/f_" + i.ToString() + "_3_joint_3d_smplify4.json";

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
                float y = joints[j][1] * scale;
                float z = joints[j][2] * scale;

                Vector3 position = new Vector3(x, y, z);
                jointList.Add(position);
            }

            // Convert
            if (modelType == BodyType.DeepRobot)
                jointData.jointList = ConvertJointDataOrder(jointList, JointData.boneIndexNamesDeepRobotJoint, JointData.boneIndexNamesOpenpose);
            else if (modelType == BodyType.SMPLify)
                jointData.jointList = ConvertJointDataOrder(jointList, JointData.boneIndexNamesSMPL, JointData.boneIndexNamesOpenpose);


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
                    Quaternion quaternion = RodriguesToQuaternion(rodVector);
                    poseList.Add(quaternion);
                }
                jointData.poseList = ConvertOpenGLToUnity(poseList);

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

        //poseList[0] = Quaternion.Euler(0, 180, 0);

        // Left Shoulder
        poseList[17] = CalcJointRotation(jointList[1], jointList[5], jointList[6]);
        // Left Elbow
        poseList[19] = CalcJointRotation(jointList[5], jointList[6], jointList[7]);

        // Right Shoulder
        poseList[16] = CalcJointRotation(jointList[1], jointList[2], jointList[3]);
        // Right Elbow
        poseList[18] = CalcJointRotation(jointList[2], jointList[3], jointList[4]);

        // TODO: 좌우 다리가 반대로 나옴

        // Left Hip
        poseList[1] = CalcJointRotation(jointList[1], jointList[12], jointList[13]);
        // Left Knee
        poseList[4] = CalcJointRotation(jointList[12], jointList[13], jointList[14]);

        // Right Hip
        poseList[2] = CalcJointRotation(jointList[1], jointList[9], jointList[10]);
        // Right Knee
        poseList[5] = CalcJointRotation(jointList[9], jointList[10], jointList[11]);
    }

    private Quaternion CalcJointRotation(Vector3 joint1, Vector3 joint2, Vector3 joint3)
    {
        Vector3 vector1 = new Vector3(joint2.x, joint2.y, joint2.z) -
                          new Vector3(joint1.x, joint1.y, joint1.z);

        Vector3 vector2 = new Vector3(joint3.x, joint3.y, joint3.z) -
                          new Vector3(joint2.x, joint2.y, joint2.z);

        return Quaternion.FromToRotation(vector1, vector2);
    }

    private Quaternion RodriguesToQuaternion(Vector3 rodVector)
    {
        float theta = rodVector.magnitude;
        if (theta < 1e-6)
        {
            return Quaternion.identity;
        }
        else
        {
            Vector3 axis = rodVector.normalized;
            return Quaternion.AngleAxis(theta * Mathf.Rad2Deg, axis);
        }
    }

    private List<Vector3> ConvertJointDataOrder(List<Vector3> jointData, string[] srcOrder, string[] destOrder)
    {
        List<Vector3> reorderJointData = new List<Vector3>(new Vector3[destOrder.Length]);
        Dictionary<string, int> srcIndexMap = new Dictionary<string, int>();

        for (int i = 0; i < srcOrder.Length; i++)
            srcIndexMap[srcOrder[i]] = i;

        for (int i = 0; i < destOrder.Length; i++)
        {
            string boneName = destOrder[i];
            if (srcIndexMap.ContainsKey(boneName))
                reorderJointData[i] = jointData[srcIndexMap[boneName]];
            else
                reorderJointData[i] = Vector3.zero;
        }

        return reorderJointData;
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

