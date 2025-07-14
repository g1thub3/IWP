#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CutsceneEditor;
using static CutsceneFunctions;

[CustomEditor(typeof(CharacterProfiles))]
public class CharacterProfileEditor : Editor
{

    CharacterProfiles myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as CharacterProfiles;
        for (int i = 0; i < myTarget.characterProfiles.Length; i++) {
            myTarget.characterProfiles[i].name = "(" + i + ") " + ((CHARACTER_ENUM)i).ToString();  
        }
    }
}
#endif