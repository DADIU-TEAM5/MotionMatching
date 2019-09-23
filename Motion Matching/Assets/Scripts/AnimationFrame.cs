using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AnimationFrame  
{
    public float Time;
    public List<AnimationJointPoint> JointPoints;
}
