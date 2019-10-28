using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

using UnityEditor.Animations;

public class PreProcess : MonoBehaviour
{
    public AnimationClips AnimationsPlay;
    public AnimationClips AllAnimations;
    public AnimationCapsules AnimationsPreProcess;

    public float Second = 1f;
    public int SaveInSecond = 10;
    public int Speed = 5;
    //Assume we know the frame rate is 30;
    public int FrameRate = 30;


    public void PreProcessTrajectory()
    {
        int count = 0;
        InitializeAnimation();
        for(int i = 0; i < AllAnimations.AnimClips.Count; i++)
        {
            if(AllAnimations.AnimClips[i].Name.Contains("InPlace"))
            {
                AnimationsPlay.AnimClips.Add(AllAnimations.AnimClips[i]);
                GetCorrespondingAnimations(AllAnimations.AnimClips[i].Name, count);
                count++;
            }
        }
    }

    private void InitializeAnimation()
    {
        var animclips = new List<AnimClip>();
        AnimationsPlay.AnimClips = animclips;
        AnimationsPreProcess.FrameCapsules = new List<Capsule>();
    }


    private void GetAnimaitionTrajectory(AnimClip animClip, int animIndex)
    {
        AnimationTrajectory.ObtainRootFromAnim(Second, SaveInSecond, FrameRate,
                      animClip, animIndex, Speed, AnimationsPreProcess.FrameCapsules);
    }

    private void GetCorrespondingAnimations(string inPlaceAnimationName, int animIndex)
    {
        var name = inPlaceAnimationName.Replace("_InPlace", "");
        for (int i = 0; i < AllAnimations.AnimClips.Count; i++)
        {
            if (AllAnimations.AnimClips[i].Name == name)
            {
                GetAnimaitionTrajectory(AllAnimations.AnimClips[i], animIndex);
                
                break;
            }
        }
    }

}
