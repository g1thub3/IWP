#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DialogueSequence))]
public class DialogueSequenceEditor : Editor
{

    DialogueSequence myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as DialogueSequence;
        foreach (var data in myTarget.sequence)
        {
            data.ImplementCharacter();
            data.name = data.speakerName;
        }
    }
}
#endif