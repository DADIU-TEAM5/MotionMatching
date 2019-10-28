using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimationByIndex : PlayerTrajectory
{
    public void PlayAnimationJoints(Vector3 rotationPlayer,
                                    CapsuleScriptObject current, Result result, 
                                    AnimationClips animationClips,
                                    Dictionary<string, Transform> skeletonJoints)

    {

        current.Capsule.AnimClipIndex = result.AnimClipIndex;
        current.Capsule.AnimClipName = result.ClipName;
        current.Capsule.FrameNum = result.FrameNum;

        if (result.FrameNum >= animationClips.AnimClips[result.AnimClipIndex].Frames.Count - 3)
            result.FrameNum = 0;
                   
        
        FrameToJoints( skeletonJoints, 
                       animationClips.AnimClips[result.AnimClipIndex].Frames[result.FrameNum]);
        transform.Rotate(rotationPlayer);
    }

    public void FrameToJoints(
                              Dictionary<string, Transform> skeletonJoints,
                              AnimationFrame frame)
    {
        foreach (var jointPoint in frame.JointPoints)
        {

            var joint = skeletonJoints[jointPoint.Name];
            ApplyJointPointToJoint(jointPoint, joint);
        }
    }


    private void ApplyJointPointToJoint( AnimationJointPoint jointPoint, Transform joint)
    {
            joint.rotation = jointPoint.Rotation;
            joint.position = transform.TransformDirection(transform.position)  + jointPoint.Position;
    }

}
