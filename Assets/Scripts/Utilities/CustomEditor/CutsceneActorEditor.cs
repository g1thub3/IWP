#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CutsceneActor))]
public class CusceneActorEditor : Editor
{
    static int dirX;
    static int dirZ;
    CutsceneActor myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as CutsceneActor;
        EditorGUILayout.TextField(CutsceneActor.Directions[myTarget.Direction]);
    }
}
#endif