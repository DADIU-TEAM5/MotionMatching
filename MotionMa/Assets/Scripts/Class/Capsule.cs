using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Capsule
{
    public string AnimClipName;
    public int FrameNum;
    public int AnimClipIndex;
    public int CapsuleIndex;
    public List<AnimationJointPoint> KeyJoints;

    public Vector3 CurrentPosition;
    public Trajectory[] TrajectoryFuture;
    public Trajectory[] TrajectoryHistory;

}
