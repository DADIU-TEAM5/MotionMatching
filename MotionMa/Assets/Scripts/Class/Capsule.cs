﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Capsule
{
    public string AnimClipName;
    public int FrameNum;

    public Vector3 CurrentPosition;
    public Vector3[] TrajectoryFuture;
    public Vector3[] TrajectoryHistory;

}