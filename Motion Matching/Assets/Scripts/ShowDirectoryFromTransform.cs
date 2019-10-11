using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDirectoryFromTransform : MonoBehaviour
{

    public AnimationClipTrajectory trajectoryData;

    public int FrameToShow = 70;

    public Transform transformToShowFrom;


    // Start is called before the first frame update
    void Start()
    {
        Debug.DrawLine(transformToShowFrom.position, transformToShowFrom.position + Vector3.forward,Color.red,10);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < trajectoryData.trajectoriesPositions[FrameToShow].Length; i++)
        {
            if (i > 10)
                Gizmos.color = Color.blue;
            

            Gizmos.DrawSphere( transformToShowFrom.TransformVector( (trajectoryData.trajectoriesPositions[FrameToShow][i]*100) + transformToShowFrom.position), 0.1f);
        }
    }
}
