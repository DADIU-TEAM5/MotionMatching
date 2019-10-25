using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MotionMatcher : MonoBehaviour
{

    // Start is called before the first frame update
    // Update is called once per frame

    public void GetMotionAndFrame(AnimationCapsules animationCapsules, CapsuleScriptObject current, 
                                    Result result, AnimationClips animationClips)
    {
        var BestFrameIndex = GetBestFrameIndex(animationCapsules, current.Capsule, animationClips);
        var bestFrame = animationCapsules.FrameCapsules[BestFrameIndex];

        bool isSameLocation = (BestFrameIndex == result.CapsuleNum) 
                                || ((bestFrame.AnimClipName == result.ClipName) 
                                && (Mathf.Abs(bestFrame.AnimClipIndex - result.AnimClipIndex) < 3));//Mathf.Abs(bestFrame.AnimClipIndex - result.AnimClipIndex) < 3
        //Debug.Log(bestFrame.AnimClipName);
        //Debug.Log(bestFrame.FrameNum);

        //for debugger!!!Warning!!
        //if (result.AnimClipIndex == 1)
        //    result.AnimClipIndex = 2;
        //if (result.AnimClipIndex == 0)
        //    result.AnimClipIndex = 3;

        if (!isSameLocation)
        {
            result.ClipName = bestFrame.AnimClipName;
            result.FrameNum = bestFrame.FrameNum;
            result.CapsuleNum = BestFrameIndex;
            result.AnimClipIndex = bestFrame.AnimClipIndex;
        }
        else
        {
            result.FrameNum++;
        }
    }


    public int GetBestFrameIndex(AnimationCapsules animationCapsules, Capsule current,
                                AnimationClips animationClips)
    {
        int BestIndex = 0;

        float bestScore = float.MaxValue;

        var bestTrajectIndexes = FindBestTrajectories(animationCapsules, current);

        for (int i = 0; i < bestTrajectIndexes.Count; i++)
        {
            var animCap = animationCapsules.FrameCapsules[bestTrajectIndexes[i]];
            var jointcost = JointsCost(animationClips.AnimClips[animCap.AnimClipIndex].Frames[animCap.FrameNum],
                                        animationClips.AnimClips[current.AnimClipIndex].Frames[current.FrameNum]);
            if (jointcost < bestScore)
            {
                bestScore = jointcost;
                BestIndex = bestTrajectIndexes[i];
            }


        }


        return BestIndex;
    }

    //private struct BestScoreandIndex
    //{
    //    public List<float> scores;
    //    public List<int> frameindex;
    //}
    private float JointsCost(AnimationFrame animation, AnimationFrame current)
    {
        float allCost = 0;
        for (int j = 0; j < animation.JointPoints.Count; j++)
        {
            allCost += BoneCost(animation.JointPoints[j], current.JointPoints[j]);
        }
        return allCost;
    }

    private float BoneCost(AnimationJointPoint frameBone, AnimationJointPoint currentBone)
    {
        var posCost = (frameBone.Position - currentBone.Position).sqrMagnitude;
        return posCost;
    }

    private List<int> FindBestTrajectories(AnimationCapsules animationCapsules, Capsule current)
    {

        int bestNum = 20;

        List<float> scores = new List<float>();
        List<int> frameindex = new List<int>();
        //initialize
        for (int i = 0; i < bestNum; i++)
        {
            scores.Add(float.MaxValue);
            frameindex.Add(0);
        }

        for (int i = 0; i < animationCapsules.FrameCapsules.Count; i++)
        {

            var score = TrajectoryCost(animationCapsules.FrameCapsules[i], current);

            //used linq
            if (scores.Max() > score)
            {
                var maxIndex = scores.IndexOf(scores.Max());
                scores[maxIndex] = score;
                frameindex[maxIndex] = i;
            }
        }

        return frameindex;
    }

    private float TrajectoryCost(Capsule frame, Capsule current)
    {
        float trajectoryCost = 0;
        //assume future length == history
        for (int i = 0; i < frame.TrajectoryFuture.Length; i++)
        {
            var futurePos = (frame.TrajectoryFuture[i] - current.TrajectoryFuture[i]).magnitude;
            var historyPos = (frame.TrajectoryHistory[i] - current.TrajectoryHistory[i]).magnitude;
            trajectoryCost += (futurePos + historyPos);
        }

        return trajectoryCost;
    }
}
