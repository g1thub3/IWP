#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Cutscene))]
public class CutsceneEditor : Editor
{
    public static Dictionary<CUTSCENE_FUNCTION, List<string>> requiredKeys;

    Cutscene myTarget;
    List<CUTSCENE_FUNCTION> selections;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //if (myTarget == null)
        //{
        //    requiredKeys = new Dictionary<CUTSCENE_FUNCTION, List<string>>();
        //    requiredKeys.Add(CUTSCENE_FUNCTION.SCENESETUP, new List<string> { "SetupKey" });
        //    requiredKeys.Add(CUTSCENE_FUNCTION.DIALOGUE, new List<string> { "DialogueSequence" });
        //    requiredKeys.Add(CUTSCENE_FUNCTION.ACTOR_FACE, new List<string> { "Actor", "Direction" });
        //    requiredKeys.Add(CUTSCENE_FUNCTION.ACTOR_MOVE, new List<string> { "Actor", "Point" });
        //    requiredKeys.Add(CUTSCENE_FUNCTION.ACTOR_ANIMATE, new List<string> { "Actor", "Anim" });
        //    myTarget = target as Cutscene;
        //    selections = new List<CUTSCENE_FUNCTION>();
        //    foreach (var instruct in myTarget.instructions) {
        //        selections.Add(instruct.function);
        //    }
        //}
        //for (int i = 0; i < selections.Count; i++)
        //{
        //    Debug.Log("Curr" + myTarget.instructions[i].function + "Old: " + selections[i]);
        //    if (myTarget.instructions[i].function != selections[i]) {
        //        selections[i] = myTarget.instructions[i].function;
        //        myTarget.instructions[i].Data.dataList.Clear();
        //        foreach (var key in requiredKeys[selections[i]])
        //        {
        //            var newEntry = new KeyDataEntry();
        //            newEntry.Key = key;
        //            myTarget.instructions[i].Data.dataList.Add(newEntry);
        //        }
        //    }
        //}
    }
}
#endif