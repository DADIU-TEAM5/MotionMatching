using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceInPlace
{
 

    public void AssignSkeleton(Transform transform)
    {
        //for (int i = 0; i < animClip.Frames.)
        var pos = transform.position;
        pos.y = 0;
        transform.position = pos;
    }
}
