using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Spline))]
public class SplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Spline spline = (Spline)target;

        if (GUILayout.Button("Add Point To Spline"))
        {
            //Will activate when pressed.
            spline.AddPosition(Vector3.zero);
        }

        if (GUILayout.Button("Update Spline"))
        {
            //Will activate when pressed.
            spline.UpdateSpline();
        }
    }
}
