using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateVelocity : MonoBehaviour
{
    float Pointangle(Transform PositionCurrent, Transform PositionLast)
    {
        float angle = Vector3.Angle(PositionCurrent.forward, PositionLast.forward);
        //Debug.Log(angle);
        return angle;
    }

    Vector3 PointVelocity(Vector3 PositionCurrent, Vector3 PositionLast)
    {
        Vector3 velocity = PositionCurrent - PositionLast;
        return velocity;
    }
}
