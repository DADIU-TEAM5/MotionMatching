using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MotionTrajectoryData
{
    public static List<float> TimeStamps;
    public static int Length() => TimeStamps?.Count ?? 0;

    public Vector3 LocalPosition;
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 Direction;
}
