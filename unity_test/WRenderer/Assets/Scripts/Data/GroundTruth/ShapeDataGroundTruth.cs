using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

// 프레임별 shape parameters를 저장할 클래스
[Serializable]
public class ShapeDataFrame
{
    public int F; // Frame index
    public List<float> S; // Shape parameters for this frame
}

// Shape fitting 정보를 저장할 클래스
[Serializable]
public class ShapeDataInfoShapeFit
{
    public float error_pose;
    public float error_shape;
    public int iter;
}

// 전체 JSON 구조를 저장할 클래스
[Serializable]
public class ShapeDataGroundTruth
{
    public string[][] _README;
    public string gender;
    public ShapeDataInfoShapeFit info_shape_fit;
    public int num_frames;
    public List<ShapeDataFrame> shape_param_frames;
    public List<float> shape_parameters;
}