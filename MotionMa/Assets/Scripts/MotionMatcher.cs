using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MotionMatcher
{

    public void GetMotionAndFrame( AnimationCapsules animationCapsules, CapsuleScriptObject current, 
                                    Result result, AnimationClips animationClips, int differentClipLength)
    {
        var bestFrameIndex = CalculateCost.GetBestFrameIndex(animationCapsules, current.Capsule, animationClips);
        var bestFrame = animationCapsules.FrameCapsules[bestFrameIndex];

        bool isSameLocation = (bestFrameIndex == result.CapsuleNum) 
                                || ((bestFrame.AnimClipName == result.ClipName) 
                                && (Mathf.Abs(bestFrame.FrameNum - result.FrameNum) < differentClipLength));


        if (!isSameLocation)
        {
            result.ClipName = bestFrame.AnimClipName;
            result.FrameNum = bestFrame.FrameNum;
            result.CapsuleNum = bestFrameIndex;
            result.AnimClipIndex = bestFrame.AnimClipIndex;
        }
        else
        {
            result.FrameNum++;
        }
    }
}
