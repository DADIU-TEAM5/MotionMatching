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
    public int CapsuleBegin;
    public int CapsuleEnd;
    public List<AnimationJointPoint> KeyJoints;
    public bool FootStep;


    public Vector3 CurrentPosition;
    public Vector3[] TrajectoryFuture;
    public Vector3[] TrajectoryHistory;

}
