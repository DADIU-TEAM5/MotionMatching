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

    public float CostWeightPosition, CostWeightVelocity, CostWeightAngle;

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

        for (int i = 0; i < amountOfNeighbours; i++) {
            var closest = CostList.Min();
            var index = CostList.IndexOf(closest);
            yield return MotionFrames[index];
            CostList[index] = float.MaxValue;
        }
    }

    private MotionFrame ClosestNeighbour(IEnumerable<MotionFrame> neighbors) {
        var orderedNeighbors = neighbors.OrderBy(x => CalculateFrameCost(x, GoalFrame.Value));

        return orderedNeighbors.First();
    }

    private void CalculateAllCost(MotionFrame currentFrame)
    {
        CostList = new List<float>(MotionFrames.Count);

        float costeachFrame;

        for (int i = 0; i < MotionFrames.Count; i++)
        {
            costeachFrame = CalculateFrameCost(currentFrame, MotionFrames[i]);
            if (costeachFrame < 1f) {
                CostList.Add(float.MaxValue);
            }  else {
                CostList.Add(costeachFrame);
            }
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

    public float CalculateFrameCost(MotionFrame CurrentFrame, MotionFrame GoalFrame)
    {
        var current = CurrentFrame.EndEffectors;
        var goal = GoalFrame.EndEffectors;

        float AllCost = 0;
        for(int i=0; i<current.Count; i++)
        {
            AllCost += CalculateOneJointCost(
                current[i].Position, goal[i].Position,
                current[i].Velocity, goal[i].Velocity, 
                current[i].Angle, goal[i].Angle);
        }
        return AllCost;
    }

    float CalculateOneJointCost(Vector3 CurrentP, Vector3 GoalP,
        Vector3 CurrentV, Vector3 GoalV, float CurrentTheta, float GoalTheta)
    {
        var costP = Vector3.Distance(CurrentP, GoalP);

        var costV = Vector3.Angle(CurrentV, GoalV);
        
        var costTheta = Mathf.Abs(CurrentTheta - GoalTheta);
        return (
            costP * CostWeightPosition 
            
            );
    } 

}
