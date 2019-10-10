


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineCreater : MonoBehaviour
{
    public AnimClipCSV AnimClip;
    public int NodeInterval;
    public BezierSpline SplineScript;
    private int _splineNumber;


    void Start()
    {
        //SplineScript.CurveCount = SplineNodes;
        

        for (int i = 0; i < AnimClip.Frames.Count; i++)
        {
            if (i % NodeInterval == 0)
            {
                for (int j = 0; j < AnimClip.Frames[i].JointPoints.Count; j++)
                {
                    // Avoid hardcoded name if possible?
                    if(AnimClip.Frames[i].JointPoints[j].Name == "Root")
                    {
                       // AnimClip.Frames[i].JointPoints.
                    }
                }
               // AnimClip.Frames[i].JointPoints.
            }
            
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
