using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum STORY_FUNCTION
{
    CREATE_SPECIAL_DATA,
    NEXT_STATE,
    PLAY_CUTSCENE
}
[CreateAssetMenu(fileName = "GameStoryFunctions", menuName = "Scriptable Objects/GameStoryFunctions")]
public class GameStoryFunctions : SingletonScriptableObject<GameStoryFunctions> // Functions to handle the story
{
    private Dictionary<STORY_FUNCTION, System.Action<StoryData, KeyDataList>> _storyFunctions;
    private void OnEnable()
    {
        _storyFunctions = new Dictionary<STORY_FUNCTION, System.Action<StoryData, KeyDataList>>();
        _storyFunctions.Add(STORY_FUNCTION.CREATE_SPECIAL_DATA, CreateSpecialData);
        _storyFunctions.Add(STORY_FUNCTION.NEXT_STATE, NextState);
        _storyFunctions.Add(STORY_FUNCTION.PLAY_CUTSCENE, PlayCutscene);
    }

    public void Invoke(STORY_FUNCTION function, StoryData data, KeyDataList dataList)
    {
        if (dataList.GetData("Scene") != null)
        {
            if (dataList.GetData("Scene").String != SceneManager.GetActiveScene().name)
            {
                return;
            }
        }
        _storyFunctions[function].Invoke(data, dataList);
    }

    private void CreateSpecialData(StoryData story,  KeyDataList keyDataList)
    {
        
        foreach (var data in keyDataList.dataList)
        {
            var newEntry = new KeyDataEntry();
            newEntry.Key = data.Key;
            story.specialData.dataList.Add(newEntry);
        }
    }

    private void NextState(StoryData story, KeyDataList keyDataList)
    {
        story.currentState++;
    }

    private void PlayCutscene(StoryData story, KeyDataList keyDataList)
    {
        var cutscene = keyDataList.GetData("Cutscene");
        if (cutscene == null) return;
        CutsceneManager.Instance.RunCutscene(cutscene.Obj as Cutscene);
    }
}
