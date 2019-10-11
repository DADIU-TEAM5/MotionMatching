using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class DebugData : ScriptableObject
{
    public MotionFrame[] frames;
    public float[] horizontalInput;
    public float[] verticalInput;
}
