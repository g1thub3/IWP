using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StoryData
{
    private static Storyline[] assets;
    public bool isCompleted;
    public string storyName;
    public int currentState;
    public KeyDataList specialData;
    public Storyline foundStoryline;
    public DGData targetDungeon;

    public void GetStory()
    {
        if (assets == null)
        {
            assets = Resources.LoadAll<Storyline>("Stories");
        }
        if (assets != null)
        {
            foreach (Storyline asset in assets)
            {
                if (asset.storyName == storyName)
                {
                    foundStoryline = asset;
                    break;
                }
            }
        }
    }
    public StoryData(string name)
    {
        isCompleted = false;
        storyName = name;
        currentState = 0;
        specialData = new KeyDataList();
        targetDungeon = null;

        GetStory();
    }
}

[Serializable]
public class StoryFunction
{
    [HideInInspector] public string name;
    public bool Yield = false;
    public STORY_FUNCTION function;
    public KeyDataList data;
}


[Serializable]
public class StoryEvent
{
    [HideInInspector] public string name;
    public int stateTrigger;
    public List<StoryFunction> functions;
    public void Invoke(StoryData data)
    {
        GameStoryFunctions.Instance.Handle(this, data);
    }
}


public enum STORY_CONTEXT
{
    ON_STORY_START,
    ON_STORY_END,
    ON_SCENE_CHANGE,
    ON_DUNGEON_PRELOAD,
    ON_DUNGEON_NEWFLOOR,
    ON_DUNGEON_COMPLETE,
    ON_QUEST_BOARD_INTERACT,
    ON_DUNGEON_LEADER_DEFEAT,
    ON_DUNGEON_PARTY_DEFEAT,
    ON_DUNGEON_ENEMIES_CLEARED,
    ON_QUEST_COMPLETE
}

[Serializable]
public class StoryContext
{
    [HideInInspector] public string name;
    public STORY_CONTEXT context;
    public List<StoryEvent> events;
    public bool Invoke(StoryData data)
    {
        bool found = false;
        foreach(var evt in events)
        {
            if (evt.stateTrigger == data.currentState)
            {
                evt.Invoke(data);
                found = true;
                break;
            }
        }
        return found;
    }
}


[CreateAssetMenu(fileName = "Storyline", menuName = "Scriptable Objects/Storyline")]
public class Storyline : ScriptableObject
{
    public string storyName;
    public string[] storyStates;
    public string GetState(int state)
    {
        return storyStates[state];
    }

    public List<StoryContext> storyContexts;

    private Dictionary<STORY_CONTEXT, StoryContext> _storyDictionary;
    private void OnEnable()
    {
        _storyDictionary = new Dictionary<STORY_CONTEXT, StoryContext>();
        foreach (var strContext in storyContexts)
        {
            _storyDictionary.Add(strContext.context, strContext);
        }
    }

    public StoryContext GetContext(STORY_CONTEXT givenContext)
    {
        if (_storyDictionary.ContainsKey(givenContext))
            return _storyDictionary[givenContext];
        return null;
    }
}
