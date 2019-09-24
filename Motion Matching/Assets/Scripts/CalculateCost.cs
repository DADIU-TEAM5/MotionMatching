using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CalculateCost : MonoBehaviour
{
    public float CostP;
    public float CostV;
    public float CostV2;
    public float CostTheta;


    float CalculateAllCost(MotionFrame CurrentFrame, MotionFrame GoalFrame)
    {
        var current = CurrentFrame.EndEffectors;
        var goal = GoalFrame.EndEffectors;

        float AllCost = 0;
        for(int i=0; i<current.Count(); i++)
        {
            AllCost += CalculateOneJointCost(current[i].Position, goal[i].Position,
                current[i].Velocity, goal[i].Velocity, current[i].Angle, goal[i].Angle);
        }
        return AllCost;
    }

    float CalculateOneJointCost(Vector3 CurrentP, Vector3 GoalP,
        Vector3 CurrentV, Vector3 GoalV, float CurrentTheta, float GoalTheta)
    {
        CostP = (CurrentP - GoalP).sqrMagnitude;
        CostV = Vector3.Angle(CurrentV, GoalV);
        //do we need this?
        CostV2 = Vector3.Angle(CurrentP - GoalP, GoalV);
        CostTheta = Mathf.Abs(CurrentTheta - GoalTheta);
        return (CostP + CostV + CostV2 + CostTheta) / 4;
    }
}
