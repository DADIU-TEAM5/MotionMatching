using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CalculateCost 
{
    public float CalculateFrameCost(MotionFrame CurrentFrame, MotionFrame GoalFrame)
    {
        var current = CurrentFrame.EndEffectors;
        var goal = GoalFrame.EndEffectors;

        float AllCost = 0;
        for(int i=0; i<current.Count; i++)
        {
            AllCost += CalculateOneJointCost(current[i].Position, goal[i].Position,
                current[i].Velocity, goal[i].Velocity, current[i].Angle, goal[i].Angle);
        }
        return AllCost;
    }

    float CalculateOneJointCost(Vector3 CurrentP, Vector3 GoalP,
        Vector3 CurrentV, Vector3 GoalV, float CurrentTheta, float GoalTheta)
    {
        var costP = Vector3.Distance(CurrentP, GoalP);

        var costV = Vector3.Distance(CurrentV, GoalV);
        
        var costTheta = Mathf.Abs(CurrentTheta - GoalTheta);
        return (costP + costV + costTheta);
    } 

}
