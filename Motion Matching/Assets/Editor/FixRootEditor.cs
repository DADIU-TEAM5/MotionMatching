using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FixRoot))]
public class FixRootEditor : Editor
{
    private FixRoot fixRoot;
    

    void OnEnable()
    {
        fixRoot = (FixRoot)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        //if (GUILayout.Button("Play Frame"))
        //{
        //    fixRoot.PlayFrame(fixRoot.test);
        //}
        
        if (GUILayout.Button("Fix Root"))
        {
            fixRoot.FixRootFrame();
        }
    }
}
