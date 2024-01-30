using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

// 프레임별 shape parameters를 저장할 클래스
[Serializable]
public class FrameShapeData
{
    public int F; // Frame index
    public List<float> S; // Shape parameters for this frame
}

// Shape fitting 정보를 저장할 클래스
[Serializable]
public class InfoShapeFit
{
    public float error_pose;
    public float error_shape;
    public int iter;
}

// 전체 JSON 구조를 저장할 클래스
[Serializable]
public class ShapeData
{
    public string[][] _README;
    public string gender;
    public InfoShapeFit info_shape_fit;
    public int num_frames;
    public List<FrameShapeData> shape_param_frames;
    public List<float> shape_parameters;
}