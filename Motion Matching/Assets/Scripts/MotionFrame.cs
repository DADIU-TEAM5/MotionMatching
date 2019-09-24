using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionFrame  
{
    public MotionJointPoint Root;
    public List<MotionJointPoint> EnderPoints = new List<MotionJointPoint>();

    public AnimationFrame AnimationFrame;
}
