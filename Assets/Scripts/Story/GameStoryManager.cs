using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "GameStoryManager", menuName = "Scriptable Objects/GameStoryManager")]
public class GameStoryManager : SingletonScriptableObject<GameStoryManager> // Handles and stores story data
{
    private List<string> _completedStories = new List<string>();
    private List<StoryData> _activeStories = new List<StoryData>();

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
        newStory.foundStoryline.OnBegin.Invoke(newStory);
    }
    public void OnSceneChange(Scene scene, LoadSceneMode mode)
    {
        if (GlobalCanvasManager.Instance == null) return;
        if (GlobalCanvasManager.Instance.IsInteractionActive) return;
        bool eventComplete = false;
        foreach (StoryData data in _activeStories)
        {
            if (eventComplete) break;
            if (data.foundStoryline == null) continue;
            if (data.foundStoryline.OnSceneChange == null) continue;
            foreach (StoryEvent storyEvent in data.foundStoryline.OnSceneChange)
            {
                if (storyEvent.stateTrigger == data.currentState)
                {
                    eventComplete = true;
                    storyEvent.Invoke(data);
                    break;
                }
            }
        }
    }
}
