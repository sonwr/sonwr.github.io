using System;
using System.Collections.Generic;
using UnityEngine;

public class JointData
{
    public List<Vector3> jointList;     // Joint (Openpose Order), size=25
    public List<Quaternion> poseList;   // SMPL pose parameter, size=24
    public List<float> shapeList;       // SMPL shape parameter, size=10

    // Ground Truth
    // 1. 3D Joint = Openpose
    // 2. SMPL = SMPL

    // DeepRobot
    // 1. 3D Joint = DeepRobot

    // SMPLify
    // 1. 3D Joint = SMPL
    // 2. SMPL = SMPL

    public static string[] boneIndexNamesSMPL = new string[] {
        "Pelvis", "L_Hip", "L_Knee", "L_Ankle", "L_Foot", "R_Hip", "R_Knee",
        "R_Ankle", "R_Foot", "SpineL", "SpineM", "SpineH", "Neck", "Head",
        "L_Collar", "L_Shoulder", "L_Elbow", "L_Wrist", "L_Hand", "R_Collar",
        "R_Shoulder", "R_Elbow", "R_Wrist", "R_Hand"
    };

    public static string[] boneIndexNamesDeepRobotJoint = new string[] {
        "Neck", "L_Shoulder", "R_Shoulder", "L_Hip", "R_Hip", "L_Elbow", "R_Elbow",
        "L_Knee", "R_Knee", "L_Wrist", "R_Wrist", "L_Ankle", "R_Ankle"
    };

    public static string[] boneIndexNamesOpenpose = new string[] {
        "Nose", "Neck", "R_Shoulder", "R_Elbow", "R_Wrist", "L_Shoulder", "L_Elbow", "L_Wrist",
        "MidHip", "R_Hip", "R_Knee", "R_Ankle", "L_Hip", "L_Knee", "L_Ankle",
        "REye", "LEye", "REar", "LEar", "LBigToe", "LSmallToe", "LHeel", "RBigToe", "RSmallToe", "RHeel"
    };


    public static int[] jointParentIndexOpenpose = new int[] { 1, -1, 1, 2, 3, 1, 5, 6, 1, 8, 9, 10, 8, 12, 13, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

    public static int jointCount = 25;
    public static int poseCount = 24;
    public static int shapeCount = 24;

    public int frameIndex = 0;

    public JointData(int frameIndex)
    {
        this.frameIndex = frameIndex;

        jointList = new List<Vector3>();
        poseList = new List<Quaternion>();
        shapeList = new List<float>();
    }
}