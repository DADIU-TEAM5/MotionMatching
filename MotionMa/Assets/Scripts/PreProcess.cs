using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PreProcess : MonoBehaviour
{
    public AnimationClips AnimationsPlay;
    public AnimationClips AllAnimations;
    public AnimationCapsules AnimationsPreProcess;

    public float Second = 1f;
    public int SaveInSecond = 10;
    public int Speed = 5;
    //Assume we know the frame rate is 100;
    public int FrameRate = 100;


    private AnimationTrajectory _animationTrajectory;


    public void PreProcessTrajectory()
    {
        InitializeAnimation();
        for(int i = 0; i < AllAnimations.AnimClips.Count; i++)
        {
            if(AllAnimations.AnimClips[i].Name.Contains("InPlace"))
            {
                GetAnimaitionTrajectory(AllAnimations.AnimClips[i], i);
                GetCorrespondingAnimations(AllAnimations.AnimClips[i].Name);
            }
        }
    }

    private void InitializeAnimation()
    {
        var animclips = new List<AnimClip>();
        AnimationsPlay.AnimClips = animclips;

        _animationTrajectory = new AnimationTrajectory();
        AnimationsPreProcess.FrameCapsules = new List<Capsule>();
    }


    private void GetAnimaitionTrajectory(AnimClip animClip, int animIndex)
    {
        _animationTrajectory.ObtainRootFromAnim(Second, SaveInSecond, FrameRate,
                      animClip, animIndex, Speed, AnimationsPreProcess.FrameCapsules);
    }

    private void GetCorrespondingAnimations(string inPlaceAnimationName)
    {
        var name = inPlaceAnimationName.Replace("_InPlace", "");
        for (int i = 0; i < AllAnimations.AnimClips.Count; i++)
        {
            if (AllAnimations.AnimClips[i].Name == name)
            {
                AnimationsPlay.AnimClips.Add(AllAnimations.AnimClips[i]);
                break;
            }
        }
    }

}
