using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrajectory : MonoBehaviour
{
    public AnimationCapsules animationCapsules;
    public CapsuleScriptObject capsule;
    public List<AnimClip> animClips;
    public float Second = 1f;
    public int SaveInSecond = 10;
    public int Speed = 5;
    //Assume we know the frame rate is 100;
    public int FrameRate = 100;


    private int SaveGap;
    private List<Capsule> capsules;

    // Start is called before the first frame update
    void Start()
    {
        SaveGap = (int)(Second * FrameRate / SaveInSecond);
        capsules = new List<Capsule>();
        for (int i = 0; i < animClips.Count; i++)
            ObtainRootFromAnim(animClips[i]);
    }

    // Update is called once per frame
    void Update()
    {
      
        
    }

    private void ObtainRootFromAnim(AnimClip animClip)
    {
        var startFrame = SaveGap * SaveInSecond;
        var endFrame = animClip.Frames.Count - SaveGap * SaveInSecond;
        

        for (int index = startFrame; index < endFrame; index++)
        {
            capsule.Capsule = new Capsule();
            var positions = new List<Vector3>();
            var fureturepositions = new List<Vector3>();
            var historypositions = new List<Vector3>();

            var currentJoint = animClip.Frames[index].JointPoints.Find(x => x.Name.Contains("Hips"));
            capsule.Capsule.CurrentPosition = currentJoint.Position;
            var currentRotation = currentJoint.Rotation;


            GetTrajectory(animClip, index, fureturepositions, historypositions);

            //assign values
            capsule.Capsule.TrajectoryFuture = fureturepositions.ToArray();
            capsule.Capsule.TrajectoryHistory = historypositions.ToArray();
            capsule.Capsule.AnimClipName = animClip.Name;
            capsule.Capsule.FrameNum = index;

            capsules.Add(capsule.Capsule);
        }
        animationCapsules.FrameCapsules = capsules;
    }

    private void GetTrajectory(AnimClip animClip, int index,
                                List<Vector3> fureturepositions,
                                List<Vector3> historypositions)
    {
        for (int i = 0; i < SaveInSecond; i++)
        {
            var futureindex = index + i * SaveGap;
            var furetureJoint = animClip.Frames[futureindex].JointPoints.Find(x => x.Name.Contains("Hips"));

            //var currentRotation = currentJoint.Rotation;
            var relativePos = furetureJoint.Position - capsule.Capsule.CurrentPosition;
            //var relativeRot = furetureJoint.Rotation * Quaternion.Inverse(currentRotation);
            fureturepositions.Add(Quaternion.Inverse(furetureJoint.Rotation) * relativePos* Speed);


            var historyIndex = index - i * SaveGap;
            var hisJoint = animClip.Frames[historyIndex].JointPoints.Find(x => x.Name.Contains("Hips"));
            relativePos = hisJoint.Position - capsule.Capsule.CurrentPosition;
            //relativeRot = hisJoint.Rotation * Quaternion.Inverse(currentRotation);
            historypositions.Add(Quaternion.Inverse(furetureJoint.Rotation) * relativePos* Speed);
        }
    }
}
