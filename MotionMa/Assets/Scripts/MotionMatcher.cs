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
        result.ClipName = "Wat";
        result.CapsuleNum = 3;
        timer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;


        if (timer > 0.5)
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

        //for debugger!!!Warning!!
        //if (result.AnimClipIndex == 1)
        //    result.AnimClipIndex = 2;
        //if (result.AnimClipIndex == 0)
        //    result.AnimClipIndex = 3;

        result.ClipName = bestFrame.AnimClipName;
        result.FrameNum = bestFrame.FrameNum;
        result.CapsuleNum = BestFrameIndex;
        result.AnimClipIndex = bestFrame.AnimClipIndex;
    }
}
