using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AnimClipCSV))]
public class AnimClipEditor : Editor
{
    override public void OnInspectorGUI() {
        base.OnInspectorGUI();

        var clip = (AnimClipCSV) target;

        if (GUILayout.Button("Reset data")) {
            clip.Frames.Clear();
            clip.ImportData();
        }
    }
}
