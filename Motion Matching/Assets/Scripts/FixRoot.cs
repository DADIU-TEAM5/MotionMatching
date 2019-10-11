using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FixRoot : MonoBehaviour
{
    public BezierSpline splineScript;
    public Transform skeleton;
    public SplineCreater splineCreater;
    //public FixRootScriptObject fixRootScript;
    private int splineIndex;
    //public int test;
    public AnimClip FixRootClip;

    private Dictionary<string, Transform> SkeletonJoints = new Dictionary<string, Transform>();
    private List<AnimationJointPoint> FrameJointPoints;

    //public void GetFixRoot()
    //{
    //   for(int i=0; i < splineScript.points.Length; i++)
    //    {
    //        fixRootScript.FixRootVectors.Add(splineScript.points[i]);
    //    }
    //}

    public void FixRootFrame()

    {

        FixRootClip.Frames.RemoveRange(0, FixRootClip.Frames.Count);
        splineIndex = 0;
        for (int i = 0; i < splineCreater.AnimClip.Frames.Count; i++)
        {
            AnimationFrame animationFrame = new AnimationFrame();
            if (i % splineCreater.NodeInterval == 0)
            {
                //Debug.Log(splineCreater.AnimClip.Frames[i].Time);
                animationFrame.Time = splineCreater.AnimClip.Frames[i].Time;
                //Debug.Log(animationFrame.Time);
                animationFrame.JointPoints = newJoints(splineCreater.AnimClip.Frames[i]);
                //Debug.Log(animationFrame.Time);

                FixRootClip.Frames.Add(animationFrame);

                //Debug.Log("0frame" + FixRootClip.Frames[0].Time);

            }
        }

    }

    private List<AnimationJointPoint> newJoints(AnimationFrame frame)
    {
        var FrameJointPoints = new List<AnimationJointPoint>();
        var hipJoints = findHip(frame);
        var newAnimationJointPoint = new AnimationJointPoint();
        
        splineIndex = 0;
        newAnimationJointPoint.Name = "Root";
        newAnimationJointPoint.Position = splineScript.points[splineIndex++];
        newAnimationJointPoint.Rotation = hipJoints.Rotation;
        newAnimationJointPoint.BaseRotation = hipJoints.BaseRotation;
        FrameJointPoints.Add(newAnimationJointPoint);


        for (int i = 0; i < frame.JointPoints.Count; i++)
        {
            FrameJointPoints.Add(frame.JointPoints[i]);
        }
        

        return FrameJointPoints;
    }


    private AnimationJointPoint findHip(AnimationFrame frame)
    {

        for (int i = 0; i < frame.JointPoints.Count; i++)
        {

            if (frame.JointPoints[i].Name.Contains("Hips"))
            {
                return frame.JointPoints[i];
            }

        }

        return frame.JointPoints[0];

    }

    public void FrameToJoints(AnimationFrame frame)
    {
        foreach (var jointPoint in frame.JointPoints)
        {
            if (!SkeletonJoints.Keys.Contains(jointPoint.Name))
            {
                continue;
            }
            //if (SkeletonJoints.Keys.Contains("Hips"))
            //    skeleton.position = splineScript.points[splineIndex++];

            var joint = SkeletonJoints[jointPoint.Name];
            ApplyJointPointToJoint(jointPoint, joint);
        }
    }


    private void ApplyJointPointToJoint(AnimationJointPoint jointPoint, Transform joint)
    {
        // Based on negative joint
        var newEulerRot = jointPoint.Rotation * Quaternion.Inverse(jointPoint.BaseRotation);
        //var newEulerRot = jointPoint.Rotation * jointPoint.BaseRotation;
        //joint.rotation = newEulerRot;
        joint.rotation = skeleton.rotation * jointPoint.Rotation;
        //joint.rotation = Skeleton.rotation * (newEulerRot);
        joint.position = skeleton.position + jointPoint.Position;

        //joint.SetPositionAndRotation(jointPoint.Position, jointPoint.Rotation);
    }

    private void transf(Transform trans)
    {
        foreach (Transform child in trans)
        {
            if (child.childCount > 0) transf(child);
            if (child.name.Contains("mixamorig:"))
            {
                child.name = child.name.Replace("mixamorig:", "");
            }
        }
    }

    private void GetAllChildren(Transform trans)
    {
        foreach (Transform child in trans)
        {
            if (child.childCount > 0) GetAllChildren(child);
            SkeletonJoints.Add(child.name, child);
        }
    }

}
