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

        GetStory();
    }
}

[Serializable]
public class StoryFunction
{
    public STORY_FUNCTION function;
    public KeyDataList data;
}


[Serializable]
public class StoryEvent
{
    public string name;
    public int stateTrigger;
    public List<StoryFunction> functions;
    public void Invoke(StoryData data)
    {
        for (int i = 0; i < functions.Count; i++)
        {
            GameStoryFunctions.Instance.Invoke(functions[i].function, data, functions[i].data);
        }
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

    public StoryEvent OnBegin;
    public List<StoryEvent> OnSceneChange;
    public StoryEvent OnEnd;
}
