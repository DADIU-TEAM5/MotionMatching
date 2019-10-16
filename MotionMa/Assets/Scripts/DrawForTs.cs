using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawForTs : MonoBehaviour
{
    
    public AnimationCapsules animationCapsules;

    public Transform transformToShowFrom;
    
    public Result result;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDrawGizmos()
    {
        //if (Index > animationCapsules.FrameCapsules.Count)
        //    Index = 0;
        //else
        //    Index++;

        //Debug.Log(Index);
        Gizmos.color = Color.red;

        for (int i = 0; i < animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryHistory.Length; i++)
        {
            Gizmos.DrawSphere(transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryHistory[i]) + transformToShowFrom.position, 0.1f);
        }

        Gizmos.color = Color.blue;
        for (int i = 0; i < animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryFuture.Length; i++)
        {
            Gizmos.DrawSphere(transformToShowFrom.TransformVector(animationCapsules.FrameCapsules[result.CapsuleNum].TrajectoryFuture[i]) + transformToShowFrom.position, 0.1f);
        }
    }
}
