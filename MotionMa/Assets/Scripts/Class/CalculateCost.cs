using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CalculateCost : MotionMatcher
{
    //todo change to piority queue or related

    public static int GetBestFrameIndex(AnimationCapsules animationCapsules, Capsule current,
                                AnimationClips animationClips, MagicMotions magicMotions)
    {
        int BestIndex = 0;

        float bestScore = float.MaxValue;

        var bestTrajectIndexes = FindBestTrajectories(animationCapsules, current,magicMotions);

        for (int i = 0; i < bestTrajectIndexes.frameindex.Count; i++)
        {
            var animCap = animationCapsules.FrameCapsules[bestTrajectIndexes.frameindex[i]];
            //var jointcost = JointsCost(animationClips.AnimClips[animCap.AnimClipIndex].Frames[animCap.FrameNum],
            //                            animationClips.AnimClips[current.AnimClipIndex].Frames[current.FrameNum]);

            var jointCost = TestCapusuleJointCost(animCap, animationCapsules.FrameCapsules[current.CapsuleIndex]);
            var sumCost = jointCost + 1f / 3f * bestTrajectIndexes.scores[bestTrajectIndexes.frameindex[i]];

            if (sumCost < bestScore)
            {
                bestScore = sumCost;
                BestIndex = bestTrajectIndexes.frameindex[i];
            }


        }


        return BestIndex;
    }

    private static float TestCapusuleJointCost(Capsule animationCapsule, Capsule current)
    {
        float allCost = 0;
        for (int j = 0; j < animationCapsule.KeyJoints.Count; j++)
        {
            allCost += BoneCost(animationCapsule.KeyJoints[j], current.KeyJoints[j]);
        }
        return allCost;
    }


    private static float JointsCost(AnimationFrame animation, AnimationFrame current)
    {
        float allCost = 0;
        for (int j = 0; j < animation.JointPoints.Count; j++)
        {
            allCost += BoneCost(animation.JointPoints[j], current.JointPoints[j]);
        }
        return allCost;
    }

    private static float BoneCost(AnimationJointPoint frameBone, AnimationJointPoint currentBone)
    {
        var posCost = (frameBone.Position - currentBone.Position).sqrMagnitude;
        return posCost;
    }

    private static ScoreWithIndex FindBestTrajectories(AnimationCapsules animationCapsules, 
                                            Capsule current, MagicMotions MagicMotionNames)
    {

        int bestNum = 50;
        ScoreWithIndex scoreWithIndex;
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
            if(IsMagicMotion(animationCapsules.FrameCapsules[i].AnimClipName,MagicMotionNames))
                continue;

            var score = TrajectoryCost(animationCapsules.FrameCapsules[i], current);

            //used linq
            if (scores.Max() > score)
            {
                var maxIndex = scores.IndexOf(scores.Max());
                scores[maxIndex] = score;
                frameindex[maxIndex] = i;
            }
        }
        scoreWithIndex.frameindex = frameindex;
        scoreWithIndex.scores = scores;

        return scoreWithIndex;
    }

    //could be update
    private static bool IsMagicMotion(string animName, MagicMotions MagicMotionNames)
    {
        for(int i = 0; i < MagicMotionNames.AttackMotions.Count; i++)
        {
            if(animName == MagicMotionNames.AttackMotions[i].AnimClipName)
                return true;
        }
        return false;
    }
    private static float TrajectoryCost(Capsule frame, Capsule current)
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


    public struct ScoreWithIndex
    {
        public List<float> scores;
        public List<int> frameindex;
    }
}
