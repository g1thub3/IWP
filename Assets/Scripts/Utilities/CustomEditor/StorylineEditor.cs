#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CutsceneFunctions;

[CustomEditor(typeof(Storyline))]
public class StorylineEditor : Editor
{
    Storyline myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as Storyline;
        myTarget.OnBegin.name = myTarget.storyStates[myTarget.OnBegin.stateTrigger];
        myTarget.OnEnd.name = myTarget.storyStates[myTarget.OnEnd.stateTrigger];
        foreach (var evt in myTarget.OnSceneChange)
        {
            evt.name = myTarget.storyStates[evt.stateTrigger];
        }
    }
}
#endif