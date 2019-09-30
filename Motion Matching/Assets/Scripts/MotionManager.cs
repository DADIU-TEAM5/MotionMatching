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
    
    private MotionFrame PlayerMotionFrame;
    
    public float CostWeightPosition, CostWeightVelocity, CostWeightAngle;
    public PlayerSetting playerSetting;

    private float timer;
    private string MotionName = "test_dash";

    public bool isJump;
    public bool isDash;


    void Awake()
    {
        MotionClips = new List<MotionClipData>();
        for (int i = 0; i < AnimationClips.Count; i++) {
            var clip = AnimationClips[i];
            ExtractMotionClips(clip);
        }
    }

    void Start() {
        //NextFrame.Value = MotionFrames[NextIndex];
        //GoalFrame.Value = MotionFrames[GoalIndex];
        PlayerMotionFrame =new MotionFrame();
    }

    void Update()
    {
        timer += Time.deltaTime;
        // TODO: Update next frame 
        GetNextFrame();
    }



    public void GetNextFrame()
    {
        var calCulateCost = new CalculateCost();
        float bestScore = float.MaxValue;
        int bestScoreClipIndex = 0;
        int bestScoreFrameIndex = 0;

       
        GetPlayerMotion(PlayerInput, timer);

        for (int i =0; i < MotionClips.Count; i++)
        {
            //var normalizedTime = (timer % MotionClips[i].MotionClipLengthInMilliseconds) / MotionClips[i].MotionClipLengthInMilliseconds;
            

            for (int j = 0; j < MotionClips[i].MotionFrames.Length; j++)
            {        
                var thisMotionScore = calCulateCost.CalculateAllCost(MotionClips[i].MotionFrames[j],
                                                                     PlayerMotionFrame, 
                                                                     playerSetting);

               
                if (thisMotionScore < 0)
                    continue;
                if (thisMotionScore < bestScore)
                {
                    bestScore = thisMotionScore;
                    bestScoreClipIndex = i;
                    bestScoreFrameIndex = j;
                }
                
            }
        }
        //PlayerMotionFrame = MotionClips[bestScoreClipIndex].MotionFrames[bestScoreFrameIndex];
        //Debug.Log(bestScore);
        NextFrame.Value = MotionClips[bestScoreClipIndex].MotionFrames[bestScoreFrameIndex];
    }

    public void GetClipTrajectoryData(MotionFrame frame) {
        frame.TrajectoryDatas = new MotionTrajectoryData[MotionTrajectoryData.Length()];

        for (var i = 0; i < MotionTrajectoryData.Length(); i++) {
            var timeStamp = 1f / (float) i;

            var trajectoryData = new MotionTrajectoryData();
            trajectoryData.LocalPosition = frame.Velocity * frame.Direction * timeStamp;
            trajectoryData.Velocity = frame.Velocity * frame.Direction;

            if (frame.AngularVelocity != 0f) {
                trajectoryData.Direction = Quaternion.Euler(0, frame.AngularVelocity * timeStamp, 0) * Vector3.forward;
            }

            PlayerMotionFrame.TrajectoryDatas[i] = trajectoryData;
        }
    }

    public void GetPlayerMotion(PlayerInput playerInput, float normalizedTime) {
        var motionFrame = GetBakedMotionFrame(playerInput, normalizedTime);
        PlayerMotionFrame.Velocity = PlayerInput.Velocity;
        //PlayerMotionFrame.Joints = motionFrame.Joints;
        PlayerMotionFrame.Joints = motionFrame.Joints;

        PlayerMotionFrame.TrajectoryDatas = new MotionTrajectoryData[MotionTrajectoryData.Length()];

        for (var i = 0; i < MotionTrajectoryData.Length(); i++) {
            var timeStamp = 1f / (float) i;

            var trajectoryData = new MotionTrajectoryData();
            trajectoryData.LocalPosition = PlayerInput.Velocity * PlayerInput.Direction * timeStamp;
            trajectoryData.Velocity = PlayerInput.Velocity * PlayerInput.Direction;

            if (PlayerInput.AngularVelocity != 0f) {
                trajectoryData.Direction = Quaternion.Euler(0, PlayerInput.AngularVelocity * timeStamp, 0) * Vector3.forward;
            }

            PlayerMotionFrame.TrajectoryDatas[i] = trajectoryData;
        }
    }

    private MotionFrame GetBakedMotionFrame(PlayerInput playerInput,
                                            float timer)//, MotionClipType motionClipType)
    {

        MotionFrame motionFrame = null;
        for (int i = 0; i < MotionClips.Count; i++)
        {
            MotionClipData motionClipData = MotionClips[i];
            var normalizedTime = (timer % MotionClips[i].MotionClipLengthInMilliseconds) / MotionClips[i].MotionClipLengthInMilliseconds;
            if (isJump || isDash)
            {
                if (isJump && motionClipData.Name.Contains("jump"))
                {
                    MotionName = motionClipData.Name;
                    motionFrame = motionClipData.MotionFrames[0];
                    break;
                }
                if (isDash && motionClipData.Name.Contains("dash"))
                {
                    MotionName = motionClipData.Name;
                    motionFrame = motionClipData.MotionFrames[0];
                    break;
                }
            }
            else if(motionClipData.Name == MotionName)
            {
                int frame = Mathf.FloorToInt(motionClipData.MotionFrames.Length * normalizedTime);
                motionFrame = motionClipData.MotionFrames[frame];
                break;
            }
    
        }
        return motionFrame;

        /*
         else if (playerInput.Crouch && motionClipData.Name.Contains("crouch"))
        {
            MotionName = motionClipData.Name;
            motionFrame = motionClipData.MotionFrames[0];
            break;
        }
         */

    }



    /*
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
    */

    public void ExtractMotionClips(AnimClip animationClip) {
        var motionClip = new MotionClipData();
        motionClip.Name = animationClip.name;
        motionClip.MotionClipLengthInMilliseconds = animationClip.ClipLengthInMilliseconds;
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
        firstMotionFrame.Time = firstFrame.Time;
        GetClipTrajectoryData(firstMotionFrame);

        motionClip.MotionFrames[0] = firstMotionFrame;
        
        // All the other ones
        for (int i = 1; i < animationClip.Frames.Count; i++) {
            var frame = animationClip.Frames[i];
            var lastFrame = animationClip.Frames[i - 1]; 
            var motionFrame = new MotionFrame();

            motionFrame.Time = frame.Time;
            
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
            GetClipTrajectoryData(motionFrame);

            motionClip.MotionFrames[i] = motionFrame;
        }

        motionClip.MotionClipLengthInMilliseconds = animationClip.Frames.Last().Time;

        MotionClips.Add(motionClip);
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
