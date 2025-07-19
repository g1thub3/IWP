using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum STORY_FUNCTION
{
    CREATE_SPECIAL_DATA,
    NEXT_STATE,
    PLAY_CUTSCENE,
    DG_ADD_TEMPMEMBER,
    REMOVE_TARGET_DUNGEON,
    SET_TARGET_DUNGEON,
    DIALOGUE,
    DG_PARTY_LOSS,
    DG_LEADER_LOSS,
    DG_END_DAY,
    DG_NEW_FLOOR,
    DG_PLAYER_WIN,
    OPEN_DUNGEON
}
[CreateAssetMenu(fileName = "GameStoryFunctions", menuName = "Scriptable Objects/GameStoryFunctions")]
public class GameStoryFunctions : SingletonScriptableObject<GameStoryFunctions> // Functions to handle the story
{
    private Dictionary<STORY_FUNCTION, System.Action<StoryData, KeyDataList>> _storyFunctions;
    private delegate bool YieldCheck();
    private YieldCheck CurrentYieldCheck;


    private void OnEnable()
    {
        _storyFunctions = new Dictionary<STORY_FUNCTION, System.Action<StoryData, KeyDataList>>();
        _storyFunctions.Add(STORY_FUNCTION.CREATE_SPECIAL_DATA, CreateSpecialData);
        _storyFunctions.Add(STORY_FUNCTION.NEXT_STATE, NextState);
        _storyFunctions.Add(STORY_FUNCTION.PLAY_CUTSCENE, PlayCutscene);
        _storyFunctions.Add(STORY_FUNCTION.DG_ADD_TEMPMEMBER, DGAddTempMember);
        _storyFunctions.Add(STORY_FUNCTION.REMOVE_TARGET_DUNGEON, RemoveTargetDungeon);
        _storyFunctions.Add(STORY_FUNCTION.SET_TARGET_DUNGEON, SetTargetDungeon);
        _storyFunctions.Add(STORY_FUNCTION.DIALOGUE, Dialogue);
        _storyFunctions.Add(STORY_FUNCTION.DG_PARTY_LOSS, DGPartyLoss);
        _storyFunctions.Add(STORY_FUNCTION.DG_LEADER_LOSS, DGLeaderLoss);
        _storyFunctions.Add(STORY_FUNCTION.DG_END_DAY, DGEndDay);
        _storyFunctions.Add(STORY_FUNCTION.DG_NEW_FLOOR, DGNewFloor);
        _storyFunctions.Add(STORY_FUNCTION.DG_PLAYER_WIN, DGPlayerWin);
        _storyFunctions.Add(STORY_FUNCTION.OPEN_DUNGEON, OpenDungeon);
    }

    private IEnumerator HandleCoroutine(StoryEvent passedEvent, StoryData data)
    {
        for (int i = 0; i < passedEvent.functions.Count; i++)
        {
            var currFunction = passedEvent.functions[i];
            Invoke(currFunction.function, data, currFunction.data);
            if (currFunction.Yield && CurrentYieldCheck != null)
            {
                while (CurrentYieldCheck.Invoke())
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    public void Handle(StoryEvent passedEvent, StoryData data)
    {
        GlobalCanvasManager.Instance.StartCoroutine(HandleCoroutine(passedEvent, data));
    }

    public void Invoke(STORY_FUNCTION function, StoryData data, KeyDataList dataList)
    {
        CurrentYieldCheck = null;
        if (dataList.GetData("Scene") != null)
        {
            if (dataList.GetData("Scene").String != SceneManager.GetActiveScene().name)
            {
                return;
            }
        }
        if (dataList.GetData("Floor") != null)
        {
            if (GameStoryManager.Instance.currentGameManager != null)
            {
                if (GameStoryManager.Instance.currentGameManager.CurrentFloor != dataList.GetData("Floor").Int)
                {
                    return;
                }
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
        CurrentYieldCheck = CutsceneManager.Instance.IsInProgress;
        CutsceneManager.Instance.RunCutscene(cutscene.Obj as Cutscene);
    }

    private void Dialogue(StoryData story, KeyDataList keyDataList)
    {
        var cutscene = keyDataList.GetData("Dialogue");
        if (cutscene == null) return;
        CurrentYieldCheck = GlobalCanvasManager.Instance.DialogueHandler.IsInProgress;
        GlobalCanvasManager.Instance.DialogueHandler.PromptSequence(cutscene.Obj as DialogueSequence);
    }

    private void RemoveTargetDungeon(StoryData story, KeyDataList keyDataList)
    {
        story.targetDungeon = null;
    }
    private void SetTargetDungeon(StoryData story, KeyDataList keyDataList)
    {
        var dungeonData = keyDataList.GetData("Dungeon");
        if (dungeonData == null) return;
        story.targetDungeon = dungeonData.Obj as DGData;
    }

    private void DGAddTempMember(StoryData story, KeyDataList keyDataList)
    {
        if (SceneManager.GetActiveScene().name != "DungeonScene") return;
        var dgGenerator = GameObject.FindFirstObjectByType<DGGenerator>();
        if (dgGenerator == null) return;
        // If not in the indicated dungeon
        if (story.targetDungeon != null)
        {
            if (GlobalGameManager.Instance.selectedDungeon != story.targetDungeon)
            {
                return;
            }
        }

        var character = keyDataList.GetData("Character");
        var startingLevel = keyDataList.GetData("Level");

        CHARACTER_ENUM chosenChar = character != null ? (CHARACTER_ENUM)character.Int : CHARACTER_ENUM.DAMSON;
        int lvl = startingLevel != null ? startingLevel.Int : 5;

        // Add party here
        dgGenerator.AddTempParty(chosenChar, lvl);
    }

    private void DGPartyLoss(StoryData story, KeyDataList keyDataList)
    {
        // Possible yield check: When player finishes input
        if (GameStoryManager.Instance.currentGameManager == null) return;
        // If not in the indicated dungeon
        if (story.targetDungeon != null)
        {
            if (GlobalGameManager.Instance.selectedDungeon != story.targetDungeon)
            {
                return;
            }
        }
        GameStoryManager.Instance.currentGameManager.PartyLoss();
    }

    private void DGLeaderLoss(StoryData story, KeyDataList keyDataList)
    {
        // Possible yield check: When player finishes input
        if (GameStoryManager.Instance.currentGameManager == null) return;
        // If not in the indicated dungeon
        if (story.targetDungeon != null)
        {
            if (GlobalGameManager.Instance.selectedDungeon != story.targetDungeon)
            {
                return;
            }
        }
        GameStoryManager.Instance.currentGameManager.PlayerLoss(true);
    }

    private void DGEndDay(StoryData story, KeyDataList keyDataList)
    {
        GlobalGameManager.Instance.DayOver = true;
    }

    private void DGNewFloor(StoryData story, KeyDataList keyDataList)
    {
        if (GameStoryManager.Instance.currentGameManager == null) return;
        if (story.targetDungeon != null)
        {
            if (GlobalGameManager.Instance.selectedDungeon != story.targetDungeon)
            {
                return;
            }
        }
        DGSeed seed = null;
        var seedData = keyDataList.GetData("Seed");
        if (seedData != null)
            seed = seedData.Obj as DGSeed;
        GameStoryManager.Instance.currentGameManager.ForceRefreshGame(seed);
    }

    private void DGPlayerWin(StoryData story, KeyDataList keyDataList)
    {
        // Possible yield check: When player finishes input
        if (GameStoryManager.Instance.currentGameManager == null) return;
        // If not in the indicated dungeon
        if (story.targetDungeon != null)
        {
            if (GlobalGameManager.Instance.selectedDungeon != story.targetDungeon)
            {
                return;
            }
        }
        GameStoryManager.Instance.currentGameManager.PlayerComplete();
    }

    private void OpenDungeon(StoryData story, KeyDataList keyDataList)
    {
        var chosen = keyDataList.GetData("Dungeon");
        if (chosen == null || chosen.Obj == null || !(chosen.Obj is DGData)) return;
        var data = chosen.Obj as DGData;
        if (!GlobalGameManager.Instance.availableDungeons.Contains(data))
            GlobalGameManager.Instance.availableDungeons.Add(data);
    }
}
