using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateCost
{
    public int GetBestFrameIndex(AnimationCapsules animationCapsules, Capsule current)
    {
        int BestIndex = 0;
        float bestScore = float.MaxValue;
       

        for(int i = 0; i< animationCapsules.FrameCapsules.Count; i++)
        {

            var score = TrajectoryCost(animationCapsules.FrameCapsules[i], current);
            if(bestScore > score)
            {
                bestScore = score;
                BestIndex = i;
            }
        }

        return BestIndex;
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
