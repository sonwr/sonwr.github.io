using System;
using System.Collections.Generic;

[Serializable]
public class FrameData
{
    public int F; // Frame Index
    public float[][] J; // Joints Data
}

[Serializable]
public class Header
{
    public List<string> Comment;
    public List<string> JOINT_NAMES;
    public int NUM_FRAMES;
    public int NUM_JOINTS;
    public List<int> PARENT_IDS;
}

[Serializable]
public class PoseData
{
    public Header Header;
    public List<FrameData> Set;
}