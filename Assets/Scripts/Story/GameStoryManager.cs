using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "GameStoryManager", menuName = "Scriptable Objects/GameStoryManager")]
public class GameStoryManager : SingletonScriptableObject<GameStoryManager> // Handles and stores story data
{
    public DGGameManager currentGameManager;

    private List<string> _completedStories = new List<string>();
    private List<StoryData> _activeStories = new List<StoryData>();

    public List<StoryData> ActiveStories
    {
        get { return _activeStories; }
    }
    private void OnEnable()
    {
        _completedStories.Clear();
        _activeStories.Clear();
    }

    public void BeginStory(string storyName)
    {
        if (_completedStories.Contains(storyName)) return;
        foreach (var story in _activeStories) {
            if (story.storyName == storyName)
                return;
        }
        var newStory = new StoryData(storyName);
        if (newStory.foundStoryline == null) return;
        _activeStories.Add(newStory);
        StoryContext onStart = newStory.foundStoryline.GetContext(STORY_CONTEXT.ON_STORY_START);
        if (onStart != null)
        {
            onStart.Invoke(newStory);
        }
    }

    private StoryContext RunStoryEvent(STORY_CONTEXT context)
    {
        if (GlobalCanvasManager.Instance == null) return null;
        if (context == STORY_CONTEXT.ON_DUNGEON_LEADER_DEFEAT)
        if (GlobalCanvasManager.Instance.IsInteractionActive) return null;
        foreach (StoryData data in _activeStories)
        {
            StoryContext onScene = data.foundStoryline.GetContext(context);
            if (onScene == null) continue;
            onScene.Invoke(data);
            return onScene;
        }
        return null;
    }

    public void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        RunStoryEvent(STORY_CONTEXT.ON_SCENE_CHANGE);
    }
    public void OnDungeonPreload()
    {
        RunStoryEvent(STORY_CONTEXT.ON_DUNGEON_PRELOAD);
    }

    public void OnDungeonNewFloor()
    {
        RunStoryEvent(STORY_CONTEXT.ON_DUNGEON_NEWFLOOR);
    }

    public bool OnDungeonComplete()
    {
        return RunStoryEvent(STORY_CONTEXT.ON_DUNGEON_COMPLETE) != null;
    }

    public bool OnQuestBoardInteract()
    {
        return RunStoryEvent(STORY_CONTEXT.ON_QUEST_BOARD_INTERACT) != null;
    }

    public bool OnDungeonDefeat(bool isLeader)
    {
        if (isLeader)
        {
            var foundEvent = RunStoryEvent(STORY_CONTEXT.ON_DUNGEON_LEADER_DEFEAT);
            Debug.Log(foundEvent);
            return foundEvent != null;
        } else
        {
            return RunStoryEvent(STORY_CONTEXT.ON_DUNGEON_PARTY_DEFEAT) != null;
        }
    }
}
