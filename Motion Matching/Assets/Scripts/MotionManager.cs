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
        FindNextFrame(); 
    }

    private IEnumerator PlayAllFrames() {
        for (int i = 0; i < MotionFrames.Count; i++) {
            var frame = MotionFrames[i];
            Debug.Log(i);
            NextFrame.Value = frame;            

            yield return null;
        }
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

            /*
            var isSameLocation = Mathf.Abs(currentFrame.AnimationFrame.Time - MotionFrames[i].AnimationFrame.Time) < 0.2f;

            if (!isSameLocation)
                CostList.Add(costeachFrame);
            else
                CostList.Add(float.MaxValue);
                */
            
            if (costeachFrame < 1f) {
                CostList.Add(float.MaxValue);
            }  else {
                CostList.Add(costeachFrame);
            }
            

        }

    }

    private void ExtractMotionClips(AnimClip animationClip) {
        // The very first frame
        var firstMotionFrame = new MotionFrame();
        var firstFrame = animationClip.Frames[0];
        var stubAnimationjointPoint = new AnimationJointPoint { Position = Vector3.zero };

        firstMotionFrame.Joints = (from jp in firstFrame.JointPoints
                                    select MakeMotionJoint(jp, stubAnimationjointPoint)).ToArray();
        foreach (var jt in firstMotionFrame.Joints) {
            jt.BaseRotation = jt.Rotation;
        }

        var rootMotionJoint = firstMotionFrame.Joints.First(x => x.Name.Equals(RootName));
        firstMotionFrame.AngularVelocity = Vector3.Angle(Vector3.forward, rootMotionJoint.Velocity) / 180f;
        firstMotionFrame.Velocity = rootMotionJoint.Velocity.sqrMagnitude;
        
        // All the other ones
        for (int i = 1; i < animationClip.Frames.Count; i++) {
            var frame = animationClip.Frames[i];
            var lastFrame = animationClip.Frames[i - 1]; 
            var motionFrame = new MotionFrame();
            
            var joints = (from jp in frame.JointPoints 
                          from jp2 in lastFrame.JointPoints
                          where jp.Name.Equals(jp2.Name)
                          select MakeMotionJoint(jp, jp2)).ToArray();
            
            foreach (var jt in joints) {
                var firstJt = firstMotionFrame.Joints.First(x => x.Name.Equals(jt.Name));
                jt.BaseRotation = firstJt.Rotation;
            }

            motionFrame.Joints = joints;
        
            var root = joints.First(x => x.Name.Equals(RootName));
            motionFrame.AngularVelocity = Vector3.Angle(Vector3.forward, root.Velocity) / 180f;
            motionFrame.Velocity = root.Velocity.sqrMagnitude;
            MotionFrames.Add(motionFrame);
        }
    }

    private MotionJointPoint MakeMotionJoint(AnimationJointPoint current, AnimationJointPoint last) {
        var motionJointPoint = new MotionJointPoint { 
            LocalPosition = current.Position,
            LocalRotation = current.Rotation,
            Rotation = current.Rotation,
            Name = current.Name, 
            Position = current.Position, 
            Velocity = current.Position - last.Position 
        };

        return motionJointPoint;
    }

    public float CalculateFrameCost(MotionFrame CurrentFrame, MotionFrame GoalFrame)
    {
        //var current = CurrentFrame.EndEffectors;
        //var goal = GoalFrame.EndEffectors;

        //var hipCost = CalculateHipCost(CurrentFrame.Root, GoalFrame.Root);
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
        //var poseCost = PoseMatch(CurrentFrame, GoalFrame);

        return 0 ; //poseCost; //+ 
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

    private float VelocityMatch(MotionJointPoint currentJ, MotionJointPoint nextJ, float time)
    {
        var frameV = (nextJ.Position - currentJ.Position)/time;
        var distanceV = (currentJ.Velocity - frameV).sqrMagnitude;
        var angleV = Vector3.Angle(currentJ.Velocity, frameV);
        return (distanceV + angleV);
    }
}
