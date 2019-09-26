using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MotionFrame  
{
    public float Velocity;

    public float AngularVelocity;

    public MotionJointPoint[] Joints;
    //trajectory data ---
    public MotionTrajectoryData[] TrajectoryDatas;
}

