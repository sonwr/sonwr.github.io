using System;
using System.Collections.Generic;
using UnityEngine;

public class JointData
{
    public List<Vector3> jointList;     // Joint (Openpose Order), size=25
    public List<Quaternion> poseList;   // SMPL pose parameter, size=24
    public List<float> shapeList;       // SMPL shape parameter, size=10


    // Ground Truth SMPL Index
    public static string[] boneIndexNamesSMPL = new string[] {
        "Pelvis", "L_Hip", "L_Knee", "L_Ankle", "L_Foot", "R_Hip", "R_Knee",
        "R_Ankle", "R_Foot", "SpineL", "SpineM", "SpineH", "Neck", "Head",
        "L_Collar", "L_Shoulder", "L_Elbow", "L_Wrist", "L_Hand", "R_Collar",
        "R_Shoulder", "R_Elbow", "R_Wrist", "R_Hand"
    };

    // DeepRobot 3D Joint Index
    public static string[] boneIndexNamesDeepRobotJoint = new string[] {
        "Neck", "L_Shoulder", "R_Shoulder", "L_Hip", "R_Hip", "L_Elbow", "R_Elbow",
        "L_Knee", "R_Knee", "L_Wrist", "R_Wrist", "L_Ankle", "R_Ankle"
    };

    // Ground Truth 3D Joint Index
    public static string[] boneIndexNamesOpenpose = new string[] {
        "Nose", "Neck", "R_Shoulder", "R_Elbow", "R_Wrist", "L_Shoulder", "L_Elbow", "L_Wrist",
        "MidHip", "R_Hip", "R_Knee", "R_Ankle", "L_Hip", "L_Knee", "L_Ankle",
        "REye", "LEye", "REar", "LEar", "LBigToe", "LSmallToe", "LHeel", "RBigToe", "RSmallToe", "RHeel"
    };

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