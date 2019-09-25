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

    public int NextIndex, GoalIndex;

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
        NextFrame.Value = MotionFrames[NextIndex];
        GoalFrame.Value = MotionFrames[GoalIndex];
        StartCoroutine(PlayAllFrames()); 
    }

    void Update()
    {
        // TODO: Update next frame 
        //FindNextFrame(); 
    }

    private IEnumerator PlayAllFrames() {
        for (int i = 0; i < MotionFrames.Count; i++) {
            var frame = MotionFrames[i];
            Debug.Log(i);
            NextFrame.Value = frame;            

            yield break;
        }
    }

    private void FindNextFrame() {
        var maximumNeighbours = 2; 
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

            var isSameLocation = (currentFrame.AnimationFrame.Time - MotionFrames[i].AnimationFrame.Time) < 0.2f;

            if(!isSameLocation)
                CostList.Add(costeachFrame);
            /*
            if (costeachFrame < 1f) {
                CostList.Add(float.MaxValue);
            }  else {
                CostList.Add(costeachFrame);
            }
            */

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
        //var current = CurrentFrame.EndEffectors;
        //var goal = GoalFrame.EndEffectors;

        var hipCost = CalculateHipCost(CurrentFrame.Root, GoalFrame.Root);
        /*
        float AllCost = 0;
        for(int i=0; i<current.Count; i++)
        {
            AllCost += CalculateOneJointCost(
                current[i].Position, goal[i].Position,
                current[i].Velocity, goal[i].Velocity, 
                current[i].Angle, goal[i].Angle);
        }
        */
        var poseCost = PoseMatch(CurrentFrame, GoalFrame);

        return poseCost + hipCost ;
    }

    private float CalculateOneJointCost(Vector3 CurrentP, Vector3 GoalP,
        Vector3 CurrentV, Vector3 GoalV, float CurrentTheta, float GoalTheta)
    {
        var costP = Vector3.Distance(CurrentP, GoalP);

        var costV = Vector3.Angle(CurrentV.normalized, (GoalP - CurrentP).normalized);
        
        var costTheta = Mathf.Abs(CurrentTheta - GoalTheta);
        return (
            costP * CostWeightPosition 
            + (180f - costV) * CostWeightVelocity 
            );
    } 

    private float CalculateHipCost(MotionJointPoint currentHip, MotionJointPoint goalHip) {
        var costY = goalHip.Position.y - currentHip.Position.y;
        
        var costV = Vector3.Angle(currentHip.Velocity.normalized, (goalHip.Position - currentHip.Position).normalized);

        return (
            costY * CostWeightPosition
            + (180f - costV) * CostWeightVelocity 
        );
    }

    private float PoseMatch(MotionFrame currentF, MotionFrame nextF)
    {
        float poseCost = 0;
        float velocityCost = 0;
        //position matching
        for (int i=0; i< currentF.EndEffectors.Count; i++)
        {
            poseCost += (currentF.EndEffectors[i].Position - nextF.EndEffectors[i].Position).sqrMagnitude;
            velocityCost += VelocityMatch(currentF.EndEffectors[i], nextF.EndEffectors[i]);
        }
        return (poseCost+velocityCost);
    }

    private float VelocityMatch(MotionJointPoint currentJ, MotionJointPoint nextJ)
    {
        var frameV = nextJ.Position - currentJ.Position;
        var distanceV = (currentJ.Velocity - frameV).sqrMagnitude;
        var angleV = Vector3.Angle(currentJ.Velocity, frameV);
        return (distanceV + angleV);
    }
}
