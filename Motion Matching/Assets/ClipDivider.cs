using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipDivider : MonoBehaviour
{
    public AnimClipCSV OriginalClip;
    public AnimClip NewClip;
    public int StartFrame, EndFrame;

    void Start()
    {
        
        for (int i = StartFrame; i < EndFrame; i++)
        {
            if (i < 0)
                break;
            if (i >= OriginalClip.Frames.Count)
                break;

            NewClip.Frames.Add(OriginalClip.Frames[i]);


            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
