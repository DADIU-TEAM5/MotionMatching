using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransToRoot))]
public class GetInPlaceAnim : Editor
{
    private TransToRoot _transToRoot;

    void OnEnable()
    {
        _transToRoot = (TransToRoot)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Get InPlace Animation"))
        {
            _transToRoot.Root2InPlace();
        }
    }
}