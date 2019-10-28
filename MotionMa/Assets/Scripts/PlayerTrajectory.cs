using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerTrajectory : MonoBehaviour
{
    public float Speed = 5;
    public float RotationSpeed = 5;
    public float Second = 1f;
    public int SaveInSecond = 10;
    public int PredictSpeed = 20;
    [Range(0.1f, 2f)]
    public float MoMaUpdateTime = 0.1f;
    [Range(0, 1)]
    public float BlendDegree = 0.5f;
    [Range(1, 30)]
    public int BlendLength = 1;
    [Range(3, 40)]
    public int DifferentClipLength = 10;


    public AnimationCapsules AnimationTrajectories;
    public AnimationClips AnimationClips;
    public Result Results;
    public bool Blend = false;
    //it is okay for static?
    public CapsuleScriptObject PlayerTrajectoryCapusule;



    private Queue<Vector3> _history = new Queue<Vector3>();
    private List<Vector3> _future = new List<Vector3>();
    private float _timer;
    private float _tempMoMaTime;
    private int _stratFrame = 3; //assume we know... todo get it!!!
    private bool _blendFlag = false;
    private int _forBlendPlay = 0;


    private MotionMatcher _motionMatcher;


    private Dictionary<string, Transform> _skeletonJoints = new Dictionary<string, Transform>();


    //i can't believe it is too long
    void Start()
    {
        GetAllChildren(transform);
       
        InitializeTrajectory();

        _motionMatcher = new MotionMatcher();

        _timer = 0;
        _tempMoMaTime = 0;
        Results.FrameNum = 0;
        Results.AnimClipIndex = 0;

        PlayerTrajectoryCapusule.Capsule = new Capsule();
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        _tempMoMaTime += Time.deltaTime;
        var inputs = Vector3.zero;

        int thisClip = Results.AnimClipIndex;
        int thisClipNum = Results.FrameNum;

        var rotationPlayer = Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed;

        Vector3 inputVel = UpdatePlayerState(inputs);
        //get player status now
        var currentPos = transform.localPosition;
        var currentRot = transform.rotation;
        HistoryTrajectory(currentPos);
        PlayerTrajectoryCapusule.Capsule.TrajectoryHistory = _history.ToArray();

        FuturePredict(currentPos, inputVel, currentRot);
        PlayerTrajectoryCapusule.Capsule.TrajectoryFuture = _future.ToArray();
        transToRelative(PlayerTrajectoryCapusule.Capsule.TrajectoryHistory, currentPos);
        transToRelative(PlayerTrajectoryCapusule.Capsule.TrajectoryFuture, currentPos);

        if (Blend)
            UpdateWithBlend(thisClip, thisClipNum, rotationPlayer);
        else
            UpdateWithoutBlend(thisClip, thisClipNum, rotationPlayer);

    }
    private void UpdateWithoutBlend(int thisClip, int thisClipNum, Vector3 rotationPlayer)
    {
        if (_tempMoMaTime > MoMaUpdateTime)
        {
            _motionMatcher.GetMotionAndFrame(AnimationTrajectories, PlayerTrajectoryCapusule,
                                              Results, AnimationClips, DifferentClipLength);
            _tempMoMaTime = 0;
            bool isSimilarMotion = ((thisClip == Results.AnimClipIndex)
                          && (Mathf.Abs(thisClipNum - Results.FrameNum) < DifferentClipLength));

            //todo if same motion, result changes (should have another struct contrl)
            if (isSimilarMotion)
            {
                Results.AnimClipIndex = thisClip;
                Results.FrameNum = thisClipNum;
                Results.FrameNum++;
            }

        }
        else
            Results.FrameNum++;


        PlayAnimationJoints(rotationPlayer, PlayerTrajectoryCapusule,
                                                Results, AnimationClips, _skeletonJoints);
        transform.Rotate(rotationPlayer);
    }


    private void UpdateWithBlend(int thisClip, int thisClipNum, Vector3 rotationPlayer)
    {
        if (_tempMoMaTime > MoMaUpdateTime)
        {
            _motionMatcher.GetMotionAndFrame(AnimationTrajectories, PlayerTrajectoryCapusule,
                                                Results, AnimationClips, DifferentClipLength);
            _tempMoMaTime = 0;

            bool isSimilarMotion = ((thisClip == Results.AnimClipIndex)
                            && (Mathf.Abs(thisClipNum - Results.FrameNum) < DifferentClipLength));


            if (isSimilarMotion)
                PlayAnimationJoints(rotationPlayer, PlayerTrajectoryCapusule,
                                                Results, AnimationClips, _skeletonJoints); 
            else
            {
                _blendFlag = true;
                PlayBlendAnimation(_skeletonJoints, BlendDegree, thisClipNum, thisClip,
                    Results, PlayerTrajectoryCapusule, AnimationClips,
                    _forBlendPlay, rotationPlayer);
            }



        }
        else if (!_blendFlag)
        {
            PlayAnimationJoints(rotationPlayer, PlayerTrajectoryCapusule,
                                                Results, AnimationClips, _skeletonJoints);
            Results.FrameNum++;
        }
        else
        {

            if (_forBlendPlay >= BlendLength)
            {
                _blendFlag = false;
                Results.FrameNum = _forBlendPlay + thisClipNum;

                _forBlendPlay = 0;
            }
            else
            {
                _forBlendPlay++;
                PlayBlendAnimation(_skeletonJoints, BlendDegree, thisClipNum, thisClip,
                     Results, PlayerTrajectoryCapusule, AnimationClips,
                     _forBlendPlay, rotationPlayer);
                //if we need play the last frame
            }
        }
    }

    private void transToRelative(Vector3[] vector3s, Vector3 current)
    {
        for (int i = 0; i < vector3s.Length; i++)
        {
            vector3s[i] = transform.InverseTransformDirection((vector3s[i] - current));
        }
        //vector3s[0] = new Vector3(0, 0, 0);
    }

    private void InitializeTrajectory()
    {
        while (_history.Count < SaveInSecond)
        {
            _history.Enqueue(transform.localPosition);
            _future.Add(transform.localPosition);
        }
    }

    private Vector3 UpdatePlayerState(Vector3 inputs)
    {
        inputs.z = Input.GetAxis("Vertical");

        //get input velocity to move
        var inputVel = inputs * Speed;
        transform.Translate(inputVel * Time.deltaTime);

        return inputVel;
    }

    private void HistoryTrajectory(Vector3 currentPos)
    {
        //save History only in the gap
        if (_timer > (Second / SaveInSecond))
        {
            _timer = 0;
            _history.Dequeue();
            _history.Enqueue(currentPos);
        }
    }

    private void FuturePredict(Vector3 currentPos, Vector3 inputVel, Quaternion currentRot)
    {
        _future[0] = currentPos;

        var rotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed * PredictSpeed);

        for (int i = 0; i < SaveInSecond; i++)
        {
            var increase = Second / SaveInSecond * i;
            var gap_increase = Quaternion.ToEulerAngles(rotation) * increase;
            var angle_increase = Quaternion.Euler(gap_increase);
            var gap = (inputVel * increase);
            var futureP = (currentPos + angle_increase * currentRot * gap);
            _future[i] = futureP;
        }

    }

    private void GetAllChildren(Transform trans)
    {
        //SkeletonJoints.Add("Root", trans);
        foreach (Transform child in trans)
        {
            if (child.childCount > 0) GetAllChildren(child);
            _skeletonJoints.Add(child.name, child);
        }
    }






    //play animation
    public void PlayAnimationJoints(Vector3 rotationPlayer,
                                    CapsuleScriptObject current, Result result,
                                    AnimationClips animationClips,
                                    Dictionary<string, Transform> skeletonJoints)

    {

        current.Capsule.AnimClipIndex = result.AnimClipIndex;
        current.Capsule.AnimClipName = result.ClipName;
        current.Capsule.FrameNum = result.FrameNum;

        if (result.FrameNum >= animationClips.AnimClips[result.AnimClipIndex].Frames.Count - 3)
            result.FrameNum = 0;


        FrameToJoints(skeletonJoints,
                       animationClips.AnimClips[result.AnimClipIndex].Frames[result.FrameNum]);
        transform.Rotate(rotationPlayer);
    }

    public void FrameToJoints(
                              Dictionary<string, Transform> skeletonJoints,
                              AnimationFrame frame)
    {
        foreach (var jointPoint in frame.JointPoints)
        {
            if (!skeletonJoints.Keys.Contains(jointPoint.Name))
            {
                continue;
            }
            var joint = skeletonJoints[jointPoint.Name];
            ApplyJointPointToJoint(jointPoint, joint);
        }
    }


    private void ApplyJointPointToJoint(AnimationJointPoint jointPoint, Transform joint)
    {
        joint.rotation = transform.rotation * jointPoint.Rotation;
        joint.position = transform.TransformDirection(jointPoint.Position) + transform.position;
    }









    public void PlayBlendAnimation(Dictionary<string, Transform> skeletonJoints, float blendDegree,
                            int beginFrameIndex, int beginAnimIndex, Result result, CapsuleScriptObject PlayerTrajectoryCapusule,
                            AnimationClips animationClips, int areadlyBlendedTimes, Vector3 rotationEular)

    {
        int bestFrameIndex = result.FrameNum;
        PlayerTrajectoryCapusule.Capsule.AnimClipIndex = result.AnimClipIndex;
        PlayerTrajectoryCapusule.Capsule.AnimClipName = result.ClipName;
        PlayerTrajectoryCapusule.Capsule.FrameNum = result.FrameNum;

        if (result.FrameNum >= animationClips.AnimClips[result.AnimClipIndex].Frames.Count - 3)//3 should be start frame 
            result.FrameNum = 3; //3 should be start frame 

        BlendAnimation(beginFrameIndex, bestFrameIndex, skeletonJoints,
            areadlyBlendedTimes, animationClips.AnimClips[beginAnimIndex], animationClips.AnimClips[result.AnimClipIndex], blendDegree);
        transform.Rotate(rotationEular);
    }


    private void BlendAnimation(int beginFrameIndex, int bestFrameIndex, Dictionary<string, Transform> skeletonJoints,
                            int areadlyBlendedTimes, AnimClip beginClip, AnimClip bestClip, float blendDegree)
    {
        var blendStart = beginFrameIndex + areadlyBlendedTimes;
        var blendEnd = bestFrameIndex + areadlyBlendedTimes;

        BlendFrame(skeletonJoints, transform, beginClip.Frames[blendStart], bestClip.Frames[blendEnd], blendDegree);
    }

    private void BlendFrame(Dictionary<string, Transform> skeletonJoints, Transform transform,
                                    AnimationFrame startFrame, AnimationFrame endFrame, float blendDegree)
    {
        for (int i = 0; i < startFrame.JointPoints.Count; i++)
        {
            var startJoint = startFrame.JointPoints[i];
            var endJoint = endFrame.JointPoints[i];

            var joint = skeletonJoints[startJoint.Name];
            BlendJoints(startJoint, endJoint, joint, blendDegree, blendDegree);
        }
    }



    private void BlendJoints(AnimationJointPoint startjointPoint, AnimationJointPoint endjointPoint,
                             Transform joint, float blendRate, float blendDegree)
    {

        joint.rotation = transform.rotation * Quaternion.Lerp(startjointPoint.Rotation, endjointPoint.Rotation, blendDegree);
        //more cost?
        joint.position = Vector3.Lerp(transform.TransformDirection(startjointPoint.Position) + transform.position,
                                        transform.TransformDirection(endjointPoint.Position) + transform.position, blendRate);
    }



}

