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

    public CapsuleScriptObject PlayerTrajectoryCapusule;

    private Queue<Vector3> history = new Queue<Vector3>();
    private List<Vector3> future = new List<Vector3>();
    private float timer;
    PlayAnimationByIndex player;




    //
    public Result result;
    public AnimationClips animationClips;
    //public Transform Skeleton;
    //public CapsuleScriptObject current;


    private Dictionary<string, Transform> SkeletonJoints = new Dictionary<string, Transform>();

    void Start()
    {
        GetAllChildren(transform);
        timer += Time.deltaTime;
        InitializeTrajectory();
        PlayerTrajectoryCapusule.Capsule = new Capsule();
       
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        var inputs = Vector3.zero;


        


        GetFrame();
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

    private void transToRelative(Vector3[] vector3s, Vector3 current)
    {
        for(int i = 0; i<vector3s.Length; i++)
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
        var rotation = Quaternion.Euler(Vector3.up * Input.GetAxis("Horizontal") * RotationSpeed * 10);

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
    public void GetFrame()

    {


        if (result.FrameNum >= animationClips.AnimClips[result.AnimClipIndex].Frames.Count)
        {
            result.FrameNum = 0;
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
        joint.position = transform.TransformDirection( jointPoint.Position) + transform.position;

        //joint.SetPositionAndRotation(jointPoint.Position, jointPoint.Rotation);
        //}
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
