using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionMatcher : MonoBehaviour
{
    CalculateCost calculateCost = new CalculateCost();
    public AnimationCapsules animationCapsules;
    public CapsuleScriptObject current;
    public AnimationClips animationClips;
    public Result result;
    private float timer;

    // Start is called before the first frame update
    void Start()
    {
        result.ClipName = "Stand";
        result.CapsuleNum = 0;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;


        if (timer > 1)
        {
            timer = 0;
            GetMotionAndFrame();
        }
        else
        {
            result.FrameNum++;
        }
    }

    private void GetMotionAndFrame()
    {
        var BestFrameIndex = calculateCost.GetBestFrameIndex(animationCapsules, current.Capsule, animationClips);
        var bestFrame = animationCapsules.FrameCapsules[BestFrameIndex];
        //Debug.Log(bestFrame.AnimClipName);
        //Debug.Log(bestFrame.FrameNum);
        result.ClipName = bestFrame.AnimClipName;
        result.FrameNum = bestFrame.FrameNum;
        result.CapsuleNum = BestFrameIndex;
        result.AnimClipIndex = bestFrame.AnimClipIndex;
    }
}
