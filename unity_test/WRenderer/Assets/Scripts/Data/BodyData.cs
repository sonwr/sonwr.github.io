using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;

public class BodyData : MonoBehaviour
{
	private string modelName;

    private List<JointData> jointFrameList;

    private List<GameObject> jointGameObjects;
    private List<GameObject> boneGameObjects;

    public BodyData(string modelName)
	{
        this.modelName = modelName;
    }

    public void Init(GameObject jointPrefab, GameObject bonePrefab, GameObject parentGameObject, Color color)
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

            if (childJoint.activeSelf == false || parentJoint == null)
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

    public void LoadFileDeepRobot(int frameStartIndex, int frameLastIndex)
    {
        jointFrameList = new List<JointData>();

        for (int i = frameStartIndex; i < frameLastIndex; i++)
        {
            JointData jointData = new JointData(i);

            // Read and process the JSON file
            string filePath = Directory.GetCurrentDirectory() + "/Data/f_" + i.ToString() + "_3_joint_3d.json";
            string dataAsJson = File.ReadAllText(filePath);
            var joints = JsonConvert.DeserializeObject<float[][]>(dataAsJson);

            // Instantiate joints and store them in the list
            List<Vector3> jointList = new List<Vector3>();
            for (int j = 0; j < joints.Length; j++)
            {
                float scale = 0.01f;

                float x = joints[j][0] * scale;
                float y = joints[j][1] * scale;
                float z = joints[j][2] * scale;

                Vector3 position = new Vector3(x, y, z);
                jointList.Add(position);
            }

            // Convert 
            jointData.jointList = ConvertJointDataOrder(jointList);

            jointFrameList.Add(jointData);
        }
    }

    private List<Vector3> ConvertJointDataOrder(List<Vector3> deepRobotData)
    {
        List<Vector3> openposeData = new List<Vector3>(new Vector3[JointData.boneIndexNamesOpenpose.Length]);
        Dictionary<string, int> deepRobotIndexMap = new Dictionary<string, int>();

        // DeepRobotJoint 인덱스 맵 구축
        for (int i = 0; i < JointData.boneIndexNamesDeepRobotJoint.Length; i++)
        {
            deepRobotIndexMap[JointData.boneIndexNamesDeepRobotJoint[i]] = i;
        }

        // Openpose 데이터로 변환
        for (int i = 0; i < JointData.boneIndexNamesOpenpose.Length; i++)
        {
            string boneName = JointData.boneIndexNamesOpenpose[i];
            if (deepRobotIndexMap.ContainsKey(boneName))
            {
                openposeData[i] = deepRobotData[deepRobotIndexMap[boneName]];
            }
            else
            {
                openposeData[i] = Vector3.zero; // 데이터가 없는 경우 Vector3.zero로 설정
            }
        }

        return openposeData;
    }
}

