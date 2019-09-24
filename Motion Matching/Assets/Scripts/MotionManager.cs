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

    void Awake()
    {
        foreach(var clip in AnimationClips) {
            ExtractMotionClips(clip);
        }
    }

    void Start() {
        NextFrame.Value = MotionFrames[2000];
        GoalFrame.Value = MotionFrames[2500];
    }

    void Update()
    {
        // TODO: Update next frame 
        FindNextFrame(); 

    }

    private void FindNextFrame() {
        var maximumNeighbours = 2; 
        var neighbors = FindNearestNeighbours(maximumNeighbours);

        var closest = ClosestNeighbour(neighbors);

        NextFrame.Value = closest;
    }

    // Now we brute forcing ^^
    private IEnumerable<MotionFrame> FindNearestNeighbours(int amountOfNeighbours) {
        var costList = CalculateAllCost(NextFrame.Value); 

        for (int i = 0; i < amountOfNeighbours; i++) {
            var closest = CostList.Min();
            var index = CostList.IndexOf(closest);
            yield return MotionFrames[index];
            CostList[index] = int.MaxValue;
        }
    }

    private MotionFrame ClosestNeighbour(IEnumerable<MotionFrame> neighbors) {
        var calcost = new CalculateCost();

        var orderedNeighbors = neighbors.OrderBy(x => calcost.CalculateFrameCost(x, GoalFrame.Value));

        return orderedNeighbors.First();
    }

    private List<float> CalculateAllCost(MotionFrame currentFrame)
    {
        var costList = new List<float>();

        float costeachFrame;

        for (int i = 0; i < MotionFrames.Count; i++)
        {
            var calcost = new CalculateCost();
            costeachFrame = calcost.CalculateFrameCost(currentFrame, MotionFrames[i]);
            CostList.Add(costeachFrame);
        }

        return costList;
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
