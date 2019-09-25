using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionClipData 
{
    public string Name;

    public MotionClipType ClipType;

    public MotionFrame[] MotionFrames;
}


public enum MotionClipType {
    Jump,
    Crouch,
    Walking,
}
