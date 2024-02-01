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

    private List<GameObject> jointGameObjects;
    private List<GameObject> boneGameObjects;

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
        string filePath = Directory.GetCurrentDirectory() + "/Data/Frameset_Joints_World3D_opose25_smooth.json";
        string jsonData = File.ReadAllText(filePath);
        JointDataGroundTruth jointDataGroundTruth = JsonConvert.DeserializeObject<JointDataGroundTruth>(jsonData);

        for(int i=0; i< jointDataGroundTruth.Set.Count; i++)
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
            jointFrameList.Add(jointData);
        }
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

            jointFrameList.Add(jointData);
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

