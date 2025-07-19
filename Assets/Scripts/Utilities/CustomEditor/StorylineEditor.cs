#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;
using static CutsceneEditor;
using static CutsceneFunctions;

[CustomEditor(typeof(Storyline))]
public class StorylineEditor : Editor
{
    public static Dictionary<STORY_FUNCTION, List<string>> requiredKeys;
    public delegate string FunctionData(StoryFunction instruction);
    public static Dictionary<STORY_FUNCTION, FunctionData> getInstructionData;

    private void OnEnable()
    {
        getInstructionData = new Dictionary<STORY_FUNCTION, FunctionData>();
        getInstructionData.Add(STORY_FUNCTION.CREATE_SPECIAL_DATA, delegate (StoryFunction instruction)
        {
            string newString = "(";
            foreach (var entry in instruction.data.dataList)
            {
                newString += " " + entry.Key + " ";
            }
            newString += ")";
            return newString;
        });
        getInstructionData.Add(STORY_FUNCTION.NEXT_STATE, delegate (StoryFunction instruction)
        {
            return string.Empty;
        });
        getInstructionData.Add(STORY_FUNCTION.PLAY_CUTSCENE, delegate (StoryFunction instruction)
        {
            string newString = "(";
            var cutsceneData = instruction.data.GetData("Cutscene");
            if (cutsceneData != null)
            {
                newString += cutsceneData.Obj.name;
            }
            else
            {
                newString += "Null";
            }
            newString += ")";
            return newString;
        });
        getInstructionData.Add(STORY_FUNCTION.DG_ADD_TEMPMEMBER, delegate (StoryFunction instruction)
        {
            string newString = "(";

            var character = instruction.data.GetData("Character");
            var startingLevel = instruction.data.GetData("Level");

            newString += (character != null ? ((CHARACTER_ENUM)character.Int).ToString() : "DAMSON") + ", ";
            newString += (startingLevel != null ? startingLevel.Int : 5);
            newString += ")";
            return newString;
        });
        getInstructionData.Add(STORY_FUNCTION.REMOVE_TARGET_DUNGEON, delegate (StoryFunction instruction)
        {
            return string.Empty;
        });
        getInstructionData.Add(STORY_FUNCTION.SET_TARGET_DUNGEON, delegate (StoryFunction instruction)
        {
            var data = instruction.data.GetData("Dungeon");
            if (data == null || data.Obj == null)
                return "(Null)";
            var dungeon = data.Obj as DGData;
            if (dungeon == null)
                return "(Null)";
            return "(" + dungeon.dungeonName + ")";
        });
        getInstructionData.Add(STORY_FUNCTION.DIALOGUE, delegate (StoryFunction instruction)
        {
            string newString = "(";
            var cutsceneData = instruction.data.GetData("Dialogue");
            if (cutsceneData != null)
            {
                newString += cutsceneData.Obj.name;
            }
            else
            {
                newString += "Null";
            }
            newString += ")";
            return newString;
        });
        getInstructionData.Add(STORY_FUNCTION.DG_PARTY_LOSS, delegate (StoryFunction instruction)
        {
            return string.Empty;
        });
        getInstructionData.Add(STORY_FUNCTION.DG_LEADER_LOSS, delegate (StoryFunction instruction)
        {
            return string.Empty;
        });
        getInstructionData.Add(STORY_FUNCTION.DG_END_DAY, delegate (StoryFunction instruction)
        {
            return string.Empty;
        });
        getInstructionData.Add(STORY_FUNCTION.DG_NEW_FLOOR, delegate (StoryFunction instruction)
        {
            string newString = "(";
            var data = instruction.data.GetData("Seed");
            if (data != null)
            {
                newString += data.Obj.name;
            }
            else
            {
                newString += "Null";
            }
            newString += ")";
            return newString;
        });
        getInstructionData.Add(STORY_FUNCTION.DG_PLAYER_WIN, delegate (StoryFunction instruction)
        {
            return string.Empty;
        });
        getInstructionData.Add(STORY_FUNCTION.OPEN_DUNGEON, delegate (StoryFunction instruction)
        {
            string newString = "(";
            var dungeonData = instruction.data.GetData("Dungeon");
            if (dungeonData != null)
            {
                newString += dungeonData.Obj.name;
            }
            else
            {
                newString += "Null";
            }
            newString += ")";
            return newString;
        });
    }

    private string GetConstants(StoryFunction instruction)
    {
        string newString = string.Empty;
        var scene = instruction.data.GetData("Scene");
        var floor = instruction.data.GetData("Floor");
        if (scene != null)
        {
            newString += "(" + scene.String + ") ";
        }
        if (floor != null)
        {
            newString += "(Floor " + floor.Int + ") ";
        }
        return newString;
    }

    Storyline myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as Storyline;
        foreach (var storyContext in myTarget.storyContexts)
        {
            storyContext.name = storyContext.context.ToString();
            foreach (var evt in storyContext.events)
            {
                evt.name = myTarget.storyStates[evt.stateTrigger];
                foreach (var func in evt.functions)
                {
                    var str = string.Format("({0}) {1} {2} {3}", func.Yield ? "Y" : "_", func.function.ToString(), getInstructionData[func.function].Invoke(func), GetConstants(func));
                    func.name = str;
                }
            }
        }
    }
}
#endif