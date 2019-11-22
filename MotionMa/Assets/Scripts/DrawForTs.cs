using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawForTs : MonoBehaviour
{
    
    public AnimationCapsules animationCapsules;

    public Transform transformToShowFrom;
    
    public Result result;


    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.yellow;

        for (int i = 0; i < animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryHistory.Length; i++)
        {
            var pos = transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryHistory[i]) + transformToShowFrom.position;
            var dir = transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryDirctionHistory[i]);
            //Gizmos.color = Color.blue;
            Gizmos.DrawSphere(pos, 0.1f);
            DrawArrow.ForGizmo(pos, dir,Color.green);
        }

        Gizmos.color = Color.green;
        for (int i = 0; i < animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryFuture.Length; i++)
        {
            var pos = transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryFuture[i]) + transformToShowFrom.position;
            var dir = transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryDirctionFuture[i]);
            Gizmos.DrawSphere(pos, 0.1f);
            DrawArrow.ForGizmo(pos, dir, Color.yellow);
            //Gizmos.DrawSphere(transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryFuture[i]) + transformToShowFrom.position, 0.1f);
        }
    }
}
