using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MotionManager : MonoBehaviour
{
    // List of all the recorded mo-cap animation
    public List<AnimClip> AnimationClips;

    // List of all frames from the animations, now more fit to motion matching
    public List<MotionFrame> MotionFrames;


    public string RootName;
    public List<string> CrucialJoints;

    public MotionFrameVariable NextFrame;
    public MotionFrameVariable GoalFrame;
    public List<float> CostList;
    public  List<MotionFrame> neighbours;
    void Awake()
    {
        foreach(var clip in AnimationClips) {
            ExtractMotionClips(clip);
        }
    }

    void Start() {
        NextFrame.Value = MotionFrames[20];
        GoalFrame.Value = MotionFrames[40];
    }

    void Update()
    {
        // TODO: Update next frame 
        FindNextFrame(); 

    }

    private void FindNextFrame() {
        var maximumNeighbours = 20; 
        FindNearestNeighbours(maximumNeighbours);
    }

    // Now we brute forcing ^^
    private void FindNearestNeighbours(int amountOfNeighbours) {
        
        int index;
        
        for (int i = 0; i < amountOfNeighbours; i++) {
            var closest = CostList.Min();
            index = CostList.IndexOf(closest);
            neighbours.Add(MotionFrames[index]);

            CostList[index] = int.MaxValue;
        }

    }

    private void CalculateAllCost()
    {
        
        float costeachFrame;
        for (int i = 0; i < MotionFrames.Count; i++)
        {
            var calcost = new CalculateCost();
            costeachFrame = calcost.CalculateFrameCost(NextFrame.Value, GoalFrame.Value);
            CostList.Add(costeachFrame);
        }
    }

    private void ExtractMotionClips(AnimClip animationClip) {
        for (int i = 1; i < animationClip.Frames.Count; i++) {
            var frame = animationClip.Frames[i];
            var lastFrame = animationClip.Frames[i - 1]; 
            var motionFrame = new MotionFrame();
            
            var root = frame.JointPoints.First(x => x.Name.Equals(RootName));
            var rootLast = lastFrame.JointPoints.First(x => x.Name.Equals(RootName));
            motionFrame.Root = MakeMotionJoint(root, rootLast);

            var crucialJoints = new List<MotionJointPoint>(); 
            foreach(var jointname in CrucialJoints) {
                var current = frame.JointPoints.First(x => x.Name.Equals(jointname));
                var last = lastFrame.JointPoints.First(x => x.Name.Equals(jointname));
                var mjp = MakeMotionJoint(current, last);
                crucialJoints.Add(mjp);
            }
            motionFrame.EndEffectors = crucialJoints;

            motionFrame.AnimationFrame = frame;

            MotionFrames.Add(motionFrame);
        }
    }

    private MotionJointPoint MakeMotionJoint(AnimationJointPoint current, AnimationJointPoint last) {
        var motionJointPoint = new MotionJointPoint { 
            Name = current.Name, 
            Position = current.Position, 
            Velocity = PointVelocity(current.Position, last.Position), Angle = PointAngle(current.Rotation, last.Rotation)
        };

        return motionJointPoint;
    }

    float PointAngle(Quaternion RotationCurrent, Quaternion RotationLast)
    {
        var angle = Quaternion.Angle(RotationCurrent, RotationLast);
        //Debug.Log(angle);
        return angle;
    }

    Vector3 PointVelocity(Vector3 PositionCurrent, Vector3 PositionLast)
    {
        Vector3 velocity = PositionCurrent - PositionLast;
        return velocity;
    }
}
