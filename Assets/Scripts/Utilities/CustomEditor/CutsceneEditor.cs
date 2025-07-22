#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static CutsceneFunctions;
using static UnityEngine.Analytics.IAnalytic;

[CustomEditor(typeof(Cutscene))]
public class CutsceneEditor : Editor
{
    public static Dictionary<CUTSCENE_FUNCTION, List<string>> requiredKeys;

    Cutscene myTarget;
    public delegate string InstructionData(CutsceneInstruction instruction);
    public static Dictionary<CUTSCENE_FUNCTION, InstructionData> getInstructionData;

    private void OnEnable()
    {
        getInstructionData = new Dictionary<CUTSCENE_FUNCTION, InstructionData>();
        getInstructionData.Add(CUTSCENE_FUNCTION.SCENESETUP, delegate(CutsceneInstruction instruction)
        {
            if (instruction.Data.GetData("SetupKey") != null)
            {
                return string.Format("({0}, {1})", instruction.Data.GetData("SetupKey").String, instruction.Data.GetData("NoRefresh") == null ? "Refresh" : "Don't Refresh");
            }
            return "(Null)";
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.DIALOGUE, delegate (CutsceneInstruction instruction)
        {
            var data = instruction.Data.GetData("DialogueSequence");
            if (data != null && data.Obj != null)
            {
                return "(" + data.Obj.name + ")";
            }
            return "(Null)";
        });

        getInstructionData.Add(CUTSCENE_FUNCTION.ACTOR_FACE, delegate (CutsceneInstruction instruction)
        {
            string newString = "(";
            if (instruction.Data.GetData("Actor") != null)
            {
                newString += instruction.Data.GetData("Actor").String + ", ";
            } else
            {
                newString += "Null, ";
            }
            if (instruction.Data.GetData("Direction") != null)
            {
                newString += CutsceneActor.Directions[instruction.Data.GetData("Direction").Int] + ")";
            }
            else
            {
                newString += "Null)";
            }
            return newString;
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.ACTOR_MOVE, delegate (CutsceneInstruction instruction)
        {
            string newString = "(";
            if (instruction.Data.GetData("Actor") != null)
            {
                newString += instruction.Data.GetData("Actor").String + ", ";
            }
            else
            {
                newString += "Null, ";
            }
            if (instruction.Data.GetData("Point") != null)
            {
                newString += instruction.Data.GetData("Point").String + ", ";
            }
            else
            {
                newString += "Null, ";
            }
            if (instruction.Data.GetData("MoveSpeed") != null)
            {
                newString += instruction.Data.GetData("MoveSpeed").Float + ")";
            }
            else
            {
                newString += "1)";
            }
            return newString;
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.ACTOR_ANIMATE, delegate (CutsceneInstruction instruction)
        {
            string newString = "(";
            if (instruction.Data.GetData("Actor") != null)
            {
                newString += instruction.Data.GetData("Actor").String + ", ";
            }
            else
            {
                newString += "Null, ";
            }
            if (instruction.Data.GetData("Anim") != null)
            {
                newString += instruction.Data.GetData("Anim").String + ")";
            }
            else
            {
                newString += "Null)";
            }
            return newString;
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.SET_CONTROLLER_POSITION, delegate (CutsceneInstruction instruction)
        {
            string newString = "(";
            if (instruction.Data.GetData("Point") != null)
            {
                newString += instruction.Data.GetData("Point").String + ")";
            }
            else
            {
                newString += "Null)";
            }
            return newString;
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.SET_CAMERA_FOCUS, delegate (CutsceneInstruction instruction)
        {
            string confine = "Confine";
            var confdata = instruction.Data.GetData("Confiner");
            if (confdata != null)
            {
                if (confdata.Int < 0)
                {
                    confine = "Don't Confine";
                }
            }
            string force = "Force";
            var forceData = instruction.Data.GetData("Force");
            if (forceData == null)
            {
                force = "Don't Force";
            }
            var pt = instruction.Data.GetData("Point");
            if (instruction.Data.GetData("Point") != null)
            {
                return string.Format("({0}, {1}, {2})", pt.String, force, confine);
            }
            var actor = instruction.Data.GetData("Actor");
            if (instruction.Data.GetData("Actor") != null)
            {
                return string.Format("({0}, {1}, {2})", actor.String, force, confine);
            }
            return "(Null)";
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.DESTROY_PROP, delegate (CutsceneInstruction instruction)
        {
            var prop = instruction.Data.GetData("Prop");
            if (prop != null)
            {
                return "(" + prop.String + ")";
            }
            var actor = instruction.Data.GetData("Actor");
            if (actor != null)
            {
                return "(" + actor.String + ")";
            }
            return "(Null)";
        });
        getInstructionData.Add(CUTSCENE_FUNCTION.WAIT, delegate (CutsceneInstruction instruction)
        {
            var time = instruction.Data.GetData("Time");
            if (time != null)
            {
                return "(" + time.Float + ")";
            }
            return "(1)";
        });
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as Cutscene;
        for (int i = 0; i < myTarget.instructions.Count; i++)
        {
            myTarget.instructions[i].name = "(" + (myTarget.instructions[i].Yield ? "Y" : "_") + ") "+ myTarget.instructions[i].function.ToString() + " " + getInstructionData[myTarget.instructions[i].function].Invoke(myTarget.instructions[i]);
        }
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