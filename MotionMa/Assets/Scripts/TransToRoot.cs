using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class TransToRoot : MonoBehaviour
{
    // Start is called before the first frame update
    public AnimClip RootMotionAnim;
    public Transform Skeleton;
    public string StopPoint;

    private Dictionary<string, Transform> _skeletonJoints;
    private void Initialize()
    {
        _skeletonJoints = new Dictionary<string, Transform>();
        GetAllChildren(Skeleton);
        //InPlaceAnim.Frames = new List<AnimationFrame>();
    }

    public void Root2InPlace()

    {
        Initialize();
        //FrameToJoints(RootMotionAnim.Frames[0]);
        // var startPoint = _skeletonJoints[StopPoint].position;
        // var startPoint = new Vector3(_skeletonJoints[StopPoint].position.x, 0, _skeletonJoints[StopPoint].position.z);
        var startPoint = new Vector3(0,0,0);
        //startPoint.y = _skeletonJoints["mixamorig:Hips"].position.y;
        //var stratRotation = _skeletonJoints["mixamorig:Hips"].rotation;
        //InPlaceAnim.Frames.Add(RootMotionAnim.Frames[0]);

        for (int i = 0; i < RootMotionAnim.Frames.Count; i++)
        {
            FrameToJoints(RootMotionAnim.Frames[i]);
            // var shift =  startPoint;
            var shift =  _skeletonJoints[StopPoint].position - startPoint;
            //keep y not moving
            //shift.y = 0;
            //Debug.Log(startPoint);
            var joints = new List<AnimationJointPoint>();
            //we can do it without play joint
            for(int j = 0; j < RootMotionAnim.Frames[i].JointPoints.Count; j++)
            {
                var joint = new AnimationJointPoint();

                joint = RootMotionAnim.Frames[i].JointPoints[j];
                joint.Position.x = RootMotionAnim.Frames[i].JointPoints[j].Position.x - shift.x;
                joint.Position.z = RootMotionAnim.Frames[i].JointPoints[j].Position.z - shift.z;
                //joint.Rotation = stratRotation * Quaternion.Inverse(RootMotionAnim.Frames[i].JointPoints[j].Rotation )* Skeleton.rotation;//;

                joints.Add(joint);
            }
            RootMotionAnim.Frames[i].JointPoints = joints;
        }
    }

    public void FrameToJoints(AnimationFrame frame)
    {
        //Debug.Log(frame.Velocity);
        //Debug.Log((int)(value * AnimationClip.Frames.Count));
        foreach (var jointPoint in frame.JointPoints)
        {
            if (!_skeletonJoints.Keys.Contains(jointPoint.Name))
            {
                //Debug.LogError($"{jointPoint.Name} is not in the {Skeleton.name}");
                continue;
            }

            var joint = _skeletonJoints[jointPoint.Name];
            ApplyJointPointToJoint(jointPoint, joint);
        }
    }


    private void ApplyJointPointToJoint(AnimationJointPoint jointPoint, Transform joint)
    {
        joint.rotation = Skeleton.rotation;
        joint.position = Skeleton.position + jointPoint.Position;
    }

    private void GetAllChildren(Transform trans)
    {
        foreach (Transform child in trans)
        {
            if (child.childCount > 0) GetAllChildren(child);
            _skeletonJoints.Add(child.name, child);
        }
    }

}
