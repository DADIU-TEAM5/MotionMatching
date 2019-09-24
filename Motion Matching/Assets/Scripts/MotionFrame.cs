using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MotionFrame  
{
    public MotionJointPoint Root;
    public List<MotionJointPoint> EndEffectors = new List<MotionJointPoint>();

    public AnimationFrame AnimationFrame;
}
