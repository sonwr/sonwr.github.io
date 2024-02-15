using System;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static List<Vector3> MultiplyYByNegativeOne(List<Vector3> vectors)
    {
        List<Vector3> vectors2 = new List<Vector3>();
        for (int i = 0; i < vectors.Count; i++)
        {
            Vector3 currentVector = vectors[i];
            vectors2.Add(new Vector3(currentVector.x, -currentVector.y, currentVector.z));
        }
        return vectors2;
    }

    public static Quaternion CalcJointRotation(Vector3 joint1, Vector3 joint2, Vector3 joint3)
    {
        Vector3 vector1 = new Vector3(joint2.x, joint2.y, joint2.z) -
                          new Vector3(joint1.x, joint1.y, joint1.z);

        Vector3 vector2 = new Vector3(joint3.x, joint3.y, joint3.z) -
                          new Vector3(joint2.x, joint2.y, joint2.z);

        return Quaternion.FromToRotation(vector1, vector2);
    }

    public static Quaternion RodriguesToQuaternion(Vector3 rodVector)
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

    public static List<Vector3> ConvertJointDataOrder(List<Vector3> jointData, string[] srcOrder, string[] destOrder)
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

    public static List<Quaternion> ConvertGroundTruthRotationToSMPL(List<Quaternion> rotationParameters)
    {
        // Convert global rotations to local rotations
        List<Quaternion> localRotations = ConvertLocalRotation(rotationParameters);

        // Reorder
        List<Quaternion> reorderedRotations = ReorderQuaternions(localRotations, JointData.boneIndexNamesGroundTruth, JointData.boneIndexNamesSMPL);

        return reorderedRotations;
    }

    private static List<Quaternion> ConvertLocalRotation(List<Quaternion> globalRotations)
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

    private static List<Quaternion> ReorderQuaternions(List<Quaternion> originalRotations, string[] srcOrder, string[] destOrder)
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

    // Calculate Mean Per Joint Position Error (MPJPE)
    public static float CalculateMPJPE(JointData a, JointData b)
    {
        List<Vector3> jointListA = a.jointList;
        List<Vector3> jointListB = b.jointList;

        if (jointListA.Count != jointListB.Count)
        {
            Debug.LogError("Lists must be of the same size.");
            return -1;
        }

        float totalDistance = 0f;
        for (int i = 0; i < jointListA.Count; i++)
        {
            totalDistance += Vector3.Distance(jointListA[i], jointListB[i]);
        }

        return totalDistance / jointListA.Count;
    }

    // Calculate Mean Per Joint Rotation Error (MPJRE)
    public static float CalculateMPJRE(JointData a, JointData b)
    {
        List<Quaternion> poseListA = a.poseList;
        List<Quaternion> poseListB = b.poseList;

        if (poseListA.Count != poseListB.Count)
        {
            Debug.LogError("Lists must be of the same size.");
            return -1;
        }

        float totalAngleDifference = 0f;
        for (int i = 0; i < poseListA.Count; i++)
        {
            float angleDiff = Quaternion.Angle(poseListA[i], poseListB[i]);
            totalAngleDifference += angleDiff;
        }

        return totalAngleDifference / poseListA.Count;
    }

    // Calculate RootE: global root error in centimeters
    public static float CalculateRootE(JointData a, JointData b)
    {
        // Calculate the distance between the predicted and actual pelvis positions
        Vector3 predictedPelvis = a.CalculatePelvisPosition();
        Vector3 actualPelvis = b.CalculatePelvisPosition();

        float distance = Vector3.Distance(predictedPelvis, actualPelvis);
        return distance;
    }

    // Calculate Mean Per Joint Position Error (MPJPE) for multiple frames
    public static float CalculateMPJPE(List<JointData> framesA, List<JointData> framesB)
    {
        if (framesA.Count != framesB.Count)
        {
            Debug.LogError("Frame lists must be of the same size.");
            return -1;
        }

        float totalError = 0f;
        for (int i = 0; i < framesA.Count; i++)
        {
            totalError += CalculateMPJPE(framesA[i], framesB[i]);
        }

        return totalError / framesA.Count;
    }

    // Calculate Mean Per Joint Rotation Error (MPJRE) for multiple frames
    public static float CalculateMPJRE(List<JointData> framesA, List<JointData> framesB)
    {
        if (framesA.Count != framesB.Count)
        {
            Debug.LogError("Frame lists must be of the same size.");
            return -1;
        }

        float totalError = 0f;
        for (int i = 0; i < framesA.Count; i++)
        {
            totalError += CalculateMPJRE(framesA[i], framesB[i]);
        }

        return totalError / framesA.Count;
    }

    // Calculate RootE: global root error in centimeters for multiple frames
    public static float CalculateRootE(List<JointData> framesA, List<JointData> framesB)
    {
        if (framesA.Count != framesB.Count)
        {
            Debug.LogError("Frame lists must be of the same size.");
            return -1;
        }

        float totalDistance = 0f;
        for (int i = 0; i < framesA.Count; i++)
        {
            totalDistance += CalculateRootE(framesA[i], framesB[i]);
        }

        return totalDistance / framesA.Count;
    }

    // Calculate temporal joint jitter for a single list of frames
    public static float CalculateTemporalJointJitter(List<JointData> frames)
    {
        if (frames.Count < 3)
        {
            Debug.LogError("Need at least 3 frames to calculate temporal jitter.");
            return -1f;
        }

        int numberOfJoints = frames[0].jointList.Count;
        float totalJitter = 0f;

        // Loop over each joint
        for (int jointIndex = 0; jointIndex < numberOfJoints; jointIndex++)
        {
            float jointJitter = 0f;

            // Loop over each frame, starting from 1 and ending at count - 1 to avoid out of range errors
            for (int frameIndex = 1; frameIndex < frames.Count - 1; frameIndex++)
            {
                Vector3 jt = frames[frameIndex].jointList[jointIndex];
                Vector3 jtMinus1 = frames[frameIndex - 1].jointList[jointIndex];
                Vector3 jtPlus1 = frames[frameIndex + 1].jointList[jointIndex];

                Vector3 jitterVec = (jt - jtMinus1 + jtPlus1) / 2f;
                jointJitter += jitterVec.sqrMagnitude; // Use squared magnitude for efficiency
            }

            // Average the jitter over all frames for this joint
            totalJitter += Mathf.Sqrt(jointJitter / (frames.Count - 2));
        }

        // Average the jitter over all joints
        return totalJitter / numberOfJoints;
    }
}
