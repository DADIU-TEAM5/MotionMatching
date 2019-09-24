using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

public class AnimationManager : MonoBehaviour
{
    public AnimClip AnimationClip;

    public Transform Skeleton;

    private Dictionary<string, Transform> SkeletonJoints = new Dictionary<string, Transform>();

    private float localTimer = 0f;

    public MotionFrameVariable NextFrame;

    void Awake() {
        GetAllChildren(Skeleton);
    }

    void Update() {
        ApplyFrameToJoints(NextFrame.Value.AnimationFrame);
    }

    public void ApplyFrameToJoints(AnimationFrame frame) {
        foreach (var jointPoint in frame.JointPoints) {
            if (!SkeletonJoints.Keys.Contains(jointPoint.Name)) {
                //Debug.LogError($"{jointPoint.Name} is not in the {Skeleton.name}");
                continue;
            }

            var joint = SkeletonJoints[jointPoint.Name];
            ApplyJointPointToJoint(jointPoint, joint);             
        } 
    }


    private void ApplyJointPointToJoint(AnimationJointPoint jointPoint, Transform joint) {
        var negativeJointPoint = AnimationClip.Frames[0].JointPoints.First(x => x.Name == jointPoint.Name);
        
        // Based on negative joint
        var newEulerRot = jointPoint.Rotation * Quaternion.Inverse(negativeJointPoint.Rotation);
        joint.rotation = newEulerRot;
        joint.position = jointPoint.Position;

        //joint.SetPositionAndRotation(jointPoint.Position, jointPoint.Rotation);
    }

    private void GetAllChildren(Transform trans) {
        foreach (Transform child in trans) {
            if (child.childCount > 0) GetAllChildren(child);
            SkeletonJoints.Add(child.name, child);
        }
    }
}
