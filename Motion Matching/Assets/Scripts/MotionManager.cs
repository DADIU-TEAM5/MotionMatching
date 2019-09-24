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

    public List<MotionFrame> NeighborsDebug;
    public List<float> CostList;

    void Awake()
    {
        for (int i = 0; i < AnimationClips.Count; i++) {
            var clip = AnimationClips[i];
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
        var maximumNeighbours = 5; 
        NeighborsDebug = FindNearestNeighbours(maximumNeighbours).ToList();

        var closest = ClosestNeighbour(NeighborsDebug);

        NextFrame.Value = closest;
    }

    // Now we brute forcing ^^
    private IEnumerable<MotionFrame> FindNearestNeighbours(int amountOfNeighbours) {
        CalculateAllCost(NextFrame.Value); 

        var zeroIndex = CostList.FindIndex(0, CostList.Count, x => x == 0f);
        CostList[zeroIndex] = int.MaxValue;

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

    private void CalculateAllCost(MotionFrame currentFrame)
    {
        CostList = new List<float>(MotionFrames.Count);

        float costeachFrame;

        for (int i = 0; i < MotionFrames.Count; i++)
        {
            var calcost = new CalculateCost();
            costeachFrame = calcost.CalculateFrameCost(currentFrame, MotionFrames[i]);
            Debug.Log(costeachFrame);
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
