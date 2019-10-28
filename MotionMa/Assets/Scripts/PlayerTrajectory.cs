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

    public AnimationCapsules animationCapsules;
    public AnimationClips animationClips;
    public Result result;
    public bool Blend = false;



    public CapsuleScriptObject PlayerTrajectoryCapusule;

    private Queue<Vector3> history = new Queue<Vector3>();
    private List<Vector3> future = new List<Vector3>();
    private float timer;

    private float _tempMoMaTime;
    private MotionMatcher _motionMatcher = new MotionMatcher();
    private int _stratFrame = 3; //assume we know... todo get it!!!
    private bool _blendFlag = false;
    private int _forBlendPlay = 0;


    PlayAnimationByIndex player;

    //public Transform Skeleton;
    //public CapsuleScriptObject current;


    private Dictionary<string, Transform> SkeletonJoints = new Dictionary<string, Transform>();

    //i can't believe it is too long
    void Start()
    {
        GetAllChildren(transform);
        timer += Time.deltaTime;
        InitializeTrajectory();
        PlayerTrajectoryCapusule.Capsule = new Capsule();
        _tempMoMaTime = 0;
        result.FrameNum = 0;
        result.AnimClipIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        _tempMoMaTime += Time.deltaTime;
        var inputs = Vector3.zero;

        int thisClip = result.AnimClipIndex;
        int thisClipNum = result.FrameNum;

        //update motion matching including the blending

        if (Blend)
            UpdateWithBlend(ref thisClip, ref thisClipNum);
        else
            UpdateWithoutBlend(ref thisClip, ref thisClipNum);

        //above moma setting

        //follows player input

        Vector3 inputVel = UpdatePlayerState(inputs);
        //get player status now
        var currentPos = transform.localPosition;
        var currentRot = transform.rotation;
        HistoryTrajectory(currentPos);
        PlayerTrajectoryCapusule.Capsule.TrajectoryHistory = history.ToArray();

        FuturePredict(currentPos, inputVel, currentRot);
        PlayerTrajectoryCapusule.Capsule.TrajectoryFuture = future.ToArray();
        transToRelative(PlayerTrajectoryCapusule.Capsule.TrajectoryHistory, currentPos);
        transToRelative(PlayerTrajectoryCapusule.Capsule.TrajectoryFuture, currentPos);


    }
    private void UpdateWithoutBlend(ref int thisClip, ref int thisClipNum)
    {
        if (_tempMoMaTime > MoMaUpdateTime)
        {
            _motionMatcher.GetMotionAndFrame(animationCapsules, PlayerTrajectoryCapusule,
                                              result, animationClips);
            //bool isSimilarMotion = ((thisClip == result.AnimClipIndex)
            //              && (Mathf.Abs(thisClipNum - result.FrameNum) < 3));

            ////todo if same motion, result changes (should have another struct contrl)
            //if (isSimilarMotion)
            //    result.FrameNum++; //play animation here
        }
        else
        {
            result.FrameNum++;

        }
        PlayAnimationJoints();
    }


    private void UpdateWithBlend(ref int thisClip, ref int thisClipNum)
    {
        if (_tempMoMaTime > MoMaUpdateTime)
        {
            thisClip = result.AnimClipIndex;
            thisClipNum = result.FrameNum;
            _motionMatcher.GetMotionAndFrame(animationCapsules, PlayerTrajectoryCapusule,
                                                result, animationClips);
            _tempMoMaTime = 0;

            bool isSimilarMotion = ((thisClip == result.AnimClipIndex)
                            && (Mathf.Abs(thisClipNum - result.FrameNum) < 3));


            if (isSimilarMotion)
                PlayAnimationJoints(); //play animation here
            else
            {
                _blendFlag = true;
                PlayBlendAnimation(thisClipNum, result.FrameNum, _forBlendPlay,
                animationClips.AnimClips[thisClip], animationClips.AnimClips[result.AnimClipIndex]);
            }



        }
        else if (!_blendFlag)
        {
            PlayAnimationJoints();
            result.FrameNum++;
        }
        else
        {

            if (_forBlendPlay >= BlendLength)
            {
                _blendFlag = false;
                result.FrameNum = _forBlendPlay + thisClipNum;

                _forBlendPlay = 0;
            }
            else
            {
                _forBlendPlay++;
                PlayBlendAnimation(thisClipNum, result.FrameNum, _forBlendPlay,
                    animationClips.AnimClips[thisClip], animationClips.AnimClips[result.AnimClipIndex]);
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
        while (history.Count < SaveInSecond)
        {
            history.Enqueue(transform.localPosition);
            future.Add(transform.localPosition);
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
        if (timer > (Second / SaveInSecond))
        {
            timer = 0;
            history.Dequeue();
            history.Enqueue(currentPos);
        }
    }

    //need to update every time by player input
    private void FuturePredict(Vector3 currentPos, Vector3 inputVel, Quaternion currentRot)
    {
        future[0] = currentPos;
        //var gap = currentRot*(inputVel * Second / SaveInSecond );
        //inputs.z = Input.GetAxis("Vertical");
        var rotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed * PredictSpeed);

        for (int i = 0; i < SaveInSecond; i++)
        {
            /*
            var increasement = Second / SaveInSecond * i;
            var gap = currentRot * (inputVel * increasement);
            var inputs_increase = increasement * inputs;
            var angle_increase = Quaternion.EulerRotation(inputs_increase);

            var futureP = (currentPos + angle_increase *gap);

            future[i] = futureP;
            */
            var increase = Second / SaveInSecond * i;
            var gap_increase = Quaternion.ToEulerAngles(rotation) * increase;
            var angle_increase = Quaternion.EulerRotation(gap_increase);
            var gap = (inputVel * increase);
            var futureP = (currentPos + angle_increase * currentRot * gap);
            future[i] = futureP;


        }

    }
    //Quaternion quaternion
    public void PlayAnimationJoints()

    {
        if (result.FrameNum >= animationClips.AnimClips[result.AnimClipIndex].Frames.Count)
        {
            //bug should get motion matching here??
            result.FrameNum = 0; //should be start frame
            PlayerTrajectoryCapusule.Capsule.AnimClipIndex = result.AnimClipIndex;
            PlayerTrajectoryCapusule.Capsule.AnimClipName = result.ClipName;
            PlayerTrajectoryCapusule.Capsule.FrameNum = result.FrameNum;

            FrameToJoints(animationClips.AnimClips[result.AnimClipIndex].Frames[result.FrameNum]);
            //transform.Rotate(Quaternion.ToEulerAngles(quaternion));
            transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);
        }
        else
        {
            PlayerTrajectoryCapusule.Capsule.AnimClipIndex = result.AnimClipIndex;
            PlayerTrajectoryCapusule.Capsule.AnimClipName = result.ClipName;
            PlayerTrajectoryCapusule.Capsule.FrameNum = result.FrameNum;
            FrameToJoints(animationClips.AnimClips[result.AnimClipIndex].Frames[result.FrameNum]);
            //transform.Rotate(Quaternion.ToEulerAngles(quaternion));
            transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);
        }

    }

    public void FrameToJoints(AnimationFrame frame)
    {
        //Debug.Log(frame.Velocity);
        //Debug.Log((int)(value * AnimationClip.Frames.Count));
        foreach (var jointPoint in frame.JointPoints)
        {
            if (!SkeletonJoints.Keys.Contains(jointPoint.Name))
            {
                //Debug.LogError($"{jointPoint.Name} is not in the {Skeleton.name}");
                continue;
            }

            var joint = SkeletonJoints[jointPoint.Name];
            ApplyJointPointToJoint(jointPoint, joint);
        }
    }


    private void ApplyJointPointToJoint(AnimationJointPoint jointPoint, Transform joint)
    {
        //if (jointPoint.Name == "Root")
        //{
        //    joint.rotation = Skeleton.rotation * jointPoint.Rotation;
        //    joint.position = Skeleton.position + jointPoint.Position;

        //}
        //else
        //{
        //var newEulerRot = jointPoint.Rotation * Quaternion.Inverse(jointPoint.BaseRotation);
        //var newEulerRot = jointPoint.Rotation * jointPoint.BaseRotation;
        //joint.rotation = newEulerRot;
        //joint.rotation = transform.rotation * Quaternion.Inverse(joint.localRotation) * jointPoint.Rotation;
        //joint.rotation = Skeleton.rotation * (newEulerRot);
        joint.rotation = transform.rotation * jointPoint.Rotation;
        joint.position = transform.TransformDirection(jointPoint.Position) + transform.position;

        //joint.SetPositionAndRotation(jointPoint.Position, jointPoint.Rotation);
        //}

        //blend every time??
        //joint.rotation = transform.rotation * jointPoint.Rotation;
        //joint.position = Vector3.Lerp(joint.position, transform.TransformDirection(jointPoint.Position) + transform.position, 0.5f);
    }



    //test once update
    //todo test each update

    public void PlayBlendAnimation(int beginFrameIndex, int bestFrameIndex,
                            int areadlyBlendedTimes, AnimClip beginClip, AnimClip bestClip)

    {
        if (result.FrameNum >= animationClips.AnimClips[result.AnimClipIndex].Frames.Count)
        {
            //bug should get motion matching here??
            result.FrameNum = 0; //should be start frame
            PlayerTrajectoryCapusule.Capsule.AnimClipIndex = result.AnimClipIndex;
            PlayerTrajectoryCapusule.Capsule.AnimClipName = result.ClipName;
            PlayerTrajectoryCapusule.Capsule.FrameNum = result.FrameNum;

            BlendAnimation(beginFrameIndex, bestFrameIndex, areadlyBlendedTimes,
                       beginClip, bestClip);
            //transform.Rotate(Quaternion.ToEulerAngles(quaternion));
            transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);
        }
        else
        {
            PlayerTrajectoryCapusule.Capsule.AnimClipIndex = result.AnimClipIndex;
            PlayerTrajectoryCapusule.Capsule.AnimClipName = result.ClipName;
            PlayerTrajectoryCapusule.Capsule.FrameNum = result.FrameNum;
            BlendAnimation(beginFrameIndex, bestFrameIndex, areadlyBlendedTimes,
                      beginClip, bestClip);
            //transform.Rotate(Quaternion.ToEulerAngles(quaternion));
            transform.Rotate(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed);
        }

    }


    private void BlendAnimation(int beginFrameIndex, int bestFrameIndex,
                            int areadlyBlendedTimes, AnimClip beginClip, AnimClip bestClip)
    {
        var blendStart = beginFrameIndex + areadlyBlendedTimes;
        var blendEnd = bestFrameIndex + areadlyBlendedTimes;

        BlendFrame(beginClip.Frames[blendStart], bestClip.Frames[blendEnd], BlendDegree);
    }

    private void BlendFrame(AnimationFrame startFrame, AnimationFrame endFrame, float blendDegree)
    {
        for (int i = 0; i < startFrame.JointPoints.Count; i++)
        {
            var startJoint = startFrame.JointPoints[i];
            if (!SkeletonJoints.Keys.Contains(startJoint.Name))
            {
                //Debug.LogError($"{jointPoint.Name} is not in the {Skeleton.name}");
                continue;
            }

            var endJoint = endFrame.JointPoints[i];
            var joint = SkeletonJoints[startJoint.Name];
            BlendJoints(startJoint, endJoint, joint, blendDegree);
        }
    }



    private void BlendJoints(AnimationJointPoint startjointPoint, AnimationJointPoint endjointPoint,
                             Transform joint, float blendRate)
    {

        joint.rotation = transform.rotation * Quaternion.Lerp(startjointPoint.Rotation, endjointPoint.Rotation, BlendDegree);
        //more cost?
        joint.position = Vector3.Lerp(transform.TransformDirection(startjointPoint.Position) + transform.position,
                                        transform.TransformDirection(endjointPoint.Position) + transform.position, blendRate);
    }

    private void GetAllChildren(Transform trans)
    {
        //SkeletonJoints.Add("Root", trans);
        foreach (Transform child in trans)
        {
            if (child.childCount > 0) GetAllChildren(child);
            SkeletonJoints.Add(child.name, child);
        }
    }

}
