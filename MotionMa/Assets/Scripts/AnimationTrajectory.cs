using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrajectory
{


    // Start is called before the first frame update
    //public void ObatainTrajectory(float second, int saveInSecond, int frameRate,
    //                            AnimationClips animationClips)
    //{
    //    SaveGap = (int)(second * frameRate / saveInSecond);
    //    capsules = new List<Capsule>();
    //    for (int i = 0; i < animationClips.AnimClips.Count; i++)
    //        ObtainRootFromAnim(animationClips.AnimClips[i],i);
    //}



    public void ObtainRootFromAnim(float second, int saveInSecond, int frameRate,
                                    AnimClip animClip, int animIndex, int speed, List<Capsule> capsules)
    {

        var saveGap = (int)(second * frameRate / saveInSecond);
        var startFrame = saveGap * saveInSecond;
        var endFrame = animClip.Frames.Count - saveGap * saveInSecond;
        //var capsules = new List<Capsule>();
        
        for (int index = startFrame; index < endFrame; index++)
        {
            var capsule = new Capsule();
            var positions = new List<Vector3>();
            var fureturepositions = new List<Vector3>();
            var historypositions = new List<Vector3>();

            var currentJoint = animClip.Frames[index].JointPoints.Find(x => x.Name.Contains("Hips"));
            capsule.CurrentPosition = currentJoint.Position;
            var currentRotation = currentJoint.Rotation;


            GetTrajectory(saveInSecond,saveGap, capsule,
                            animClip, index, speed,
                            fureturepositions, historypositions);

            //assign values
            capsule.TrajectoryFuture = fureturepositions.ToArray();
            capsule.TrajectoryHistory = historypositions.ToArray();
            capsule.AnimClipName = animClip.Name;
            capsule.FrameNum = index;
            capsule.AnimClipIndex = animIndex;

            capsules.Add(capsule);
        }
 
    }

    private void GetTrajectory(int saveInSecond, int saveGap, Capsule currentCapsule,
                                AnimClip animClip, int index, int speed,
                                List<Vector3> fureturepositions,
                                List<Vector3> historypositions)
    {
        for (int i = 0; i < saveInSecond; i++)
        {
            var futureindex = index + i * saveGap;
            var furetureJoint = animClip.Frames[futureindex].JointPoints.Find(x => x.Name.Contains("Hips"));

            //var currentRotation = currentJoint.Rotation;
            var relativePos = furetureJoint.Position - currentCapsule.CurrentPosition;
            //var relativeRot = furetureJoint.Rotation * Quaternion.Inverse(currentRotation);
            fureturepositions.Add(Quaternion.Inverse(furetureJoint.Rotation) * relativePos* speed);


            var historyIndex = index - i * saveGap;
            var hisJoint = animClip.Frames[historyIndex].JointPoints.Find(x => x.Name.Contains("Hips"));
            relativePos = hisJoint.Position - currentCapsule.CurrentPosition;
            //relativeRot = hisJoint.Rotation * Quaternion.Inverse(currentRotation);
            historypositions.Add(Quaternion.Inverse(furetureJoint.Rotation) * relativePos* speed);
        }
    }
}
