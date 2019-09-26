using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MotionManager : MonoBehaviour
{
    public PlayerInput PlayerInput;

    // List of all the recorded mo-cap animation
    public List<AnimClip> AnimationClips;

    // List of all frames from the animations, now more fit to motion matching
    public List<MotionClipData> MotionClips;

    public string RootName;

    public MotionFrameVariable NextFrame;
    public MotionFrameVariable GoalFrame;
    
    private MotionFrame PlayerMotionFrame;

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
        //NextFrame.Value = MotionFrames[NextIndex];
        //GoalFrame.Value = MotionFrames[GoalIndex];
    }

    void Update()
    {
        // TODO: Update next frame 
    }

    public MotionTrajectoryData[] GetClipTrajectoryData(MotionClipData clipData) {
        var trajectoryDatas = new MotionTrajectoryData[MotionTrajectoryData.Length()];

        for (var i = 0; i < MotionTrajectoryData.Length(); i++) {
            var timeStamp = 1f / i;
            var timeFrame = Mathf.FloorToInt(timeStamp * clipData.MotionFrames.Length);
            var frame = clipData.MotionFrames[timeFrame];

            var trajectoryData = new MotionTrajectoryData();
            trajectoryData.LocalPosition = frame.Velocity * frame.Direction * timeStamp;
            trajectoryData.Velocity = frame.Velocity * frame.Direction;

            if (frame.AngularVelocity != 0f) {
                trajectoryData.Direction = Quaternion.Euler(0, frame.AngularVelocity * timeStamp, 0) * Vector3.forward;
            }
 
        }

        return trajectoryDatas;
    }

    public void GetPlayerMotion(string motionName, float normalizedTime, MotionClipType clipType) {
        var motionFrame = GetBakedMotionFrame(motionName, normalizedTime, clipType); 

        PlayerMotionFrame.Velocity = PlayerInput.Velocity;
        PlayerMotionFrame.Joints = motionFrame.Joints;
        PlayerMotionFrame.TrajectoryDatas = new MotionTrajectoryData[MotionTrajectoryData.Length()];

        for (var i = 0; i < MotionTrajectoryData.Length(); i++) {
            var timeStamp = MotionTrajectoryData.TimeStamps[i];

            var trajectoryData = new MotionTrajectoryData();
            trajectoryData.LocalPosition = PlayerInput.Velocity * PlayerInput.Direction * timeStamp;
            trajectoryData.Velocity = PlayerInput.Velocity * PlayerInput.Direction;

            if (PlayerInput.AngularVelocity != 0f) {
                trajectoryData.Direction = Quaternion.Euler(0, PlayerInput.AngularVelocity * timeStamp, 0) * Vector3.forward;
            }
        }
    }

    private MotionFrame GetBakedMotionFrame(string motionName, float normalizedTime, MotionClipType clipType)
    {
        for (int i = 0; i < MotionClips.Count; i++) {
            var clip = MotionClips[i];
            
            if (clipType != null) {
                if (clipType == clip.ClipType) {
                    return clip.MotionFrames[0];
                }
            } else if (clip.Name.Equals(motionName)) {
                int frameBasedOnTime = Mathf.FloorToInt(clip.MotionFrames.Length * normalizedTime);

                return clip.MotionFrames[frameBasedOnTime];
            }
        }

        return null;
    }

    private void ExtractMotionClips(AnimClip animationClip) {
        var motionClip = new MotionClipData();
        motionClip.Name = animationClip.name;
        motionClip.ClipType = animationClip.ClipType;
        motionClip.MotionFrames = new MotionFrame[animationClip.Frames.Count];

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
        firstMotionFrame.Direction = rootMotionJoint.Velocity.normalized;


        motionClip.MotionFrames[0] = firstMotionFrame;
        
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
            motionFrame.Direction = root.Velocity.normalized;

            motionClip.MotionFrames[i] = motionFrame;
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
}
