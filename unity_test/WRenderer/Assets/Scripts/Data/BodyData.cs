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
            Vector3 joint = jointData.jointList[i];
            jointGameObjects[i].transform.localPosition = joint;

            if (joint == Vector3.zero)
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
    GameObject FindActiveParent(int currentIndex)
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
        if (modelType == BodyType.DeepRobot || modelType == BodyType.SMPLify)
            LoadFileDeepRobotOrSMPLify(frameStartIndex, frameLastIndex);
    }

    public void LoadFileDeepRobotOrSMPLify(int frameStartIndex, int frameLastIndex)
    {
        jointFrameList = new List<JointData>();

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
}

