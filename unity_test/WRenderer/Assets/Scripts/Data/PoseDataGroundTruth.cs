using System;
using System.Collections.Generic;


[System.Serializable]
public class PoseDataFrame
{
    public int F; // Frame index
    public List<List<float>> R; // Rotations as quaternions
}

[System.Serializable]
public class PoseDataGroundTruth
{
    public List<string> BoneNames;
    public List<int> BoneParents;
    public string[][] _README;
    public int num_frames;
    public List<PoseDataFrame> pose_parameters; // List of pose parameters for each frame
}