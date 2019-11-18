using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrajectory : PreProcess
{

    public static void ObtainRootFromAnim(float second, int saveInSecond, int frameRate, ref int countCapsuleNum,
                                    AnimClip animClip, int animIndex, int speed, List<Capsule> capsules, float maxSpeedInAnim)
    {

        var saveGap = (int)(second * frameRate / saveInSecond);
        var startFrame = saveGap * saveInSecond;
        var endFrame = animClip.Frames.Count - saveGap * saveInSecond;
        //var capsules = new List<Capsule>();
        var capsuleBegin = countCapsuleNum;
        var capsuleEnd = countCapsuleNum + endFrame - startFrame - 1;

        //int footStepBefore, footStep, footStepBehind;//

        for (int index = startFrame; index < endFrame; index++)
        {

            var capsule = new Capsule();
            var positions = new List<Vector3>();
            var fureturepositions = new List<Trajectory>();
            var historypositions = new List<Trajectory>();

            var currentJoint = animClip.Frames[index].JointPoints.Find(x => x.Name.Contains("Hips"));
            capsule.CurrentPosition = currentJoint.Position;
            var currentRotation = currentJoint.Rotation;


            GetTrajectory(saveInSecond,saveGap, capsule,
                            animClip, index, speed,
                            fureturepositions, historypositions, maxSpeedInAnim);

            //assign values
            capsule.TrajectoryFuture = fureturepositions.ToArray();
            capsule.TrajectoryHistory = historypositions.ToArray();
            capsule.AnimClipName = animClip.Name;
            capsule.FrameNum = index;
            capsule.AnimClipIndex = animIndex;
            capsule.CapsuleIndex = countCapsuleNum++;
            capsule.CapsuleBegin = capsuleBegin;
            capsule.CapsuleEnd = capsuleEnd;
            //key joints
            //for (int i = 0; i < animClip.Frames[index].JointPoints.Count; i++)
            //{
            //    capsule.KeyJoints.Add(animClip.Frames[index].JointPoints[i]);
            //}
            GetKeyJoints(animClip.Frames[index], ref capsule);
            capsules.Add(capsule);
        }
 
    }

    private static void GetTrajectory(int saveInSecond, int saveGap, Capsule currentCapsule,
                                AnimClip animClip, int index, int speed,
                                List<Trajectory> fureturepositions,
                                List<Trajectory> historypositions, float maxSpeedInAnim)
    {
        List<Vector3> futurePositionsList = new List<Vector3>();
        List<Vector3> historyPositionsList = new List<Vector3>();

        for (int i = 0; i < saveInSecond + 1; i++)
        {
            var futureindex = index + i * saveGap;
            var furetureJoint = animClip.Frames[futureindex].JointPoints.Find(x => x.Name.Contains("Hips"));


            var futureRelativePos = (furetureJoint.Position - currentCapsule.CurrentPosition) * speed * maxSpeedInAnim;
            //futureRelativePos.y = 0; // assum we have no jump now
            var futureRotatedBackPos = Quaternion.Inverse(furetureJoint.Rotation) * futureRelativePos;
            futureRotatedBackPos.y = 0;


            futurePositionsList.Add(futureRotatedBackPos);


            //same for history
            var historyIndex = index - i * saveGap;
            var hisJoint = animClip.Frames[historyIndex].JointPoints.Find(x => x.Name.Contains("Hips"));

            var hisRelativePos = (hisJoint.Position - currentCapsule.CurrentPosition) * speed * maxSpeedInAnim;
            //hisRelativePos.y = 0;
            var hisRotatedBackPos = Quaternion.Inverse(hisJoint.Rotation) * hisRelativePos;
            hisRotatedBackPos.y = 0;
            historyPositionsList.Add(hisRotatedBackPos);
        }

        for (int i = 0; i < saveInSecond; i++)
        {
            var trajectory = new Trajectory();
            trajectory.Position = futurePositionsList[i];
            trajectory.Direction = futurePositionsList[i] - futurePositionsList[i + 1];
            fureturepositions.Add(trajectory);
        }

        for (int i = 0; i < saveInSecond; i++)
        {
            var trajectory = new Trajectory();
            trajectory.Position = historyPositionsList[i];
            trajectory.Direction = historyPositionsList[i + 1] - historyPositionsList[i];
            historypositions.Add(trajectory);
        }

    }
    private static void GetKeyJoints(AnimationFrame animationFrame, ref Capsule capsule)
    {
        capsule.KeyJoints = new List<AnimationJointPoint>();
        for (int i = 0; i < animationFrame.JointPoints.Count; i++)
        {
            if (animationFrame.JointPoints[i].Name == "Hips")
                continue;

            capsule.KeyJoints.Add(animationFrame.JointPoints[i]);
        }
    }

}
