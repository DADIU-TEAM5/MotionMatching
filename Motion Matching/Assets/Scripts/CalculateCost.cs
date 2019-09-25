using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CalculateCost 
{

    public float CalculateAllCost(List<MotionFrame> motionFrames, MotionFrame currentFrame,
                                    PlayerSetting playerSetting)
    {
        float allCost = 0;
        for (int i=0; i < motionFrames.Count; i++)
        {
            allCost += RootMotionCost(motionFrames[i], currentFrame, playerSetting);
            for(int j = 0; j< motionFrames[i].Joints.Length; j ++)
            {
                allCost += BoneCost(motionFrames[i].Joints[j], currentFrame.Joints[j], playerSetting);
            }
        }
        return allCost;
    }
       // the frame is from clips
    private float RootMotionCost(MotionFrame frame, MotionFrame current, 
        PlayerSetting playerSetting)
    {
        var velocity = Mathf.Abs(frame.Velocity - current.Velocity);
        return (playerSetting.RootMotionCostFactor * velocity);
    }


    //frameBone is the bone we look at, which is from animation clips
    private float BoneCost(MotionJointPoint frameBone, MotionJointPoint currentBone, 
                            PlayerSetting playerSetting)
    {
        var rotationCost = RotationCost(frameBone, currentBone);
        var posCost = PosCost(frameBone, currentBone);
        return playerSetting.BoneRotFactor * rotationCost + playerSetting.BonePosFactor * posCost;
    }

    private float PosCost(MotionJointPoint frameBone, MotionJointPoint currentBone)
    {
        var posCost = (frameBone.LocalPosition - currentBone.LocalPosition).sqrMagnitude;
        return posCost;
    }

    private float RotationCost(MotionJointPoint frameBone, MotionJointPoint currentBone)
    {
        var bonePosRotation = Quaternion.Inverse(frameBone.LocalRotation) * currentBone.LocalRotation;
        var rotationCost = Mathf.Abs(bonePosRotation.x) + Mathf.Abs(bonePosRotation.x)
                        + Mathf.Abs(bonePosRotation.y) + (1 - Mathf.Abs(bonePosRotation.w));
        return rotationCost;
    }


    //private float Trajectory(MotionJointPoint FrameBone, MotionJointPoint CurrentBone,
    // PlayerSetting PlayerSettings)
}
