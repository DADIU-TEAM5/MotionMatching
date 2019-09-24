using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionManager : MonoBehaviour
{
    // List of all the recorded mo-cap animation
    public List<AnimClip> AnimationClips;

    // List of all frames from the animations, now more fit to motion matching
    public List<MotionFrame> MotionClips;

    void Awake()
    {
        foreach(var clip in AnimationClips) {
            ExtractMotionClips(clip);
        }
    }

    void Update()
    {
        
    }

    private void ExtractMotionClips(AnimClip animationClip) {
        for (int i = 0; i < animationClip.Frames.Count; i++) {
            var frame = animationClip.Frames[i];

        }
    }
}
