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
        ScoreWithIndex jointScore;
        //jointScore.frameindex = bestTrajectIndexes.frameindex;
        jointScore.scores = new List<float>();

        for (int i = 0; i < bestTrajectIndexes.capsuleIndex.Count; i++)
        {
            //frameIndex seems capsule index
            var animCap = animationCapsules.FrameCapsules[bestTrajectIndexes.capsuleIndex[i]];
            //var jointcost = JointsCost(animationClips.AnimClips[animCap.AnimClipIndex].Frames[animCap.FrameNum],
            //                            animationClips.AnimClips[current.AnimClipIndex].Frames[current.FrameNum]);
            // see if it is correct for current index
            var jointCost = TestCapusuleJointCost(animCap, animationCapsules.FrameCapsules[current.CapsuleIndex]);
            jointScore.scores.Add(jointCost);
        }


        //normalized to do Change the Linq To our calculation
        //these could be done with preprocess
        var minScore = jointScore.scores.Min();
        var gapScore = jointScore.scores.Max() - minScore;
        for (int i = 0; i < jointScore.scores.Count; i++)
            jointScore.scores[i] = (jointScore.scores[i] - minScore) / gapScore;

        for(int i = 0; i < jointScore.scores.Count; i++) {
            var sumScore = jointScore.scores[i] * 2 + bestTrajectIndexes.scores[i];

            //for debug
            //if (animationCapsules.FrameCapsules[bestTrajectIndexes.capsuleIndex[i]].AnimClipName.Contains("Idle_R"))
            //{
            //    Debug.Log("Idle_R joint score" + jointScore.scores[BestIndex]);
            //    Debug.Log("Idle_R trajectory score" + bestTrajectIndexes.scores[BestIndex]);
            //    Debug.Log("sum idle r" + (bestTrajectIndexes.scores[BestIndex] + jointScore.scores[BestIndex]));
            //}

            if (sumScore < bestScore)
            {
                bestScore = sumScore;
                BestIndex = i;
            }
        }
        //for debug
        //if (animationCapsules.FrameCapsules[bestTrajectIndexes.capsuleIndex[BestIndex]].AnimClipName.Contains("Run"))
        //{
        //    Debug.Log("joint score" + jointScore.scores[BestIndex]);
        //    //Debug.Log("trajectory score" + bestTrajectIndexes.scores[BestIndex]);
        //    //Debug.Log("sum score" + bestScore);
        //}
        //if (animationCapsules.FrameCapsules[bestTrajectIndexes.capsuleIndex[BestIndex]].AnimClipName.Contains("Idle_L"))
        //{
        //    Debug.Log("joint score" + jointScore.scores[BestIndex]);
        //    Debug.Log("trajectory score" + bestTrajectIndexes.scores[BestIndex]);
        //    Debug.Log("sum score" + bestScore);
        //}

        return bestTrajectIndexes.capsuleIndex[BestIndex];
    }

    private static float TestCapusuleJointCost(Capsule animationCapsule, Capsule current)
    {
        float allCost = 0;
        for (int j = 1; j < animationCapsule.KeyJoints.Count; j++)
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


        //normalized
        var minScore = scores.Min();
        var gapScore = scores.Max() - minScore;
        for (int i = 0; i < bestNum; i++)
            scores[i] = (scores[i] - minScore)/ gapScore;

        scoreWithIndex.capsuleIndex = frameindex;
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
            var futurePos = Vector3.Distance(frame.TrajectoryFuture[i] , current.TrajectoryFuture[i]);
            var historyPos = Vector3.Distance(frame.TrajectoryHistory[i] , current.TrajectoryHistory[i]);

            //direction
            var futurePosDir = Vector3.Distance(frame.TrajectoryDirctionFuture[i], current.TrajectoryDirctionFuture[i]);
            var historyPosDir = Vector3.Distance(frame.TrajectoryDirctionHistory[i], current.TrajectoryDirctionHistory[i]);

            trajectoryCost += ((futurePos + historyPos) + (futurePosDir + historyPosDir));
            //trajectoryCost += ((futurePos ) + (futurePosDir) );
        }

        return trajectoryCost;
    }


    public struct ScoreWithIndex
    {
        public List<float> scores;
        public List<int> capsuleIndex;
    }
}
