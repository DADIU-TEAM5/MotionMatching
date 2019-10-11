using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(SplineCreater))]
public class DrawRoot : Editor
{
    private SplineCreater splineCreater;

    void OnEnable()
    {
        splineCreater = (SplineCreater)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        splineCreater.NodeInterval = (int)EditorGUILayout.Slider("NodeInterval",
                                    splineCreater.NodeInterval, 0, 100);

        if (GUILayout.Button("Draw Trajectory"))
        {
            splineCreater.DrawTrajectory();
        }
    }
}
