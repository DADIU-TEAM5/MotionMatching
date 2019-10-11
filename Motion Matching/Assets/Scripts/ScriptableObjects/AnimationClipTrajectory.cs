using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AnimationClipTrajectory : ScriptableObject
{
    public string name;
    public int StartFrame, EndFrame;

    public List<Vector3[]> trajectoriesPositions;
    public List<Quaternion[]> trajectoryRotations;


}
