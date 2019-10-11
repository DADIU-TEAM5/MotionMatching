using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(ClipsFetch))]
public class FetchEditor : Editor
{
    private ClipsFetch clipsFetch;

    void OnEnable()
    {
        clipsFetch = (ClipsFetch)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        clipsFetch.value = EditorGUILayout.Slider("frame", clipsFetch.value, 0f, 0.9999f);
        clipsFetch.GetFrame();
    }
}
