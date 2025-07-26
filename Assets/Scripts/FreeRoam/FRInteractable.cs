using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FRInteraction
{
    bool hasInvoked = false;
    bool wasObjectInvoked = false;
    public Object objectToInteract;
    public UnityEvent function;
    
    public void Invoke()
    {
        hasInvoked = true;
        if (objectToInteract != null)
        {
            wasObjectInvoked = true;
            if (objectToInteract is DialogueSequence)
            {
                var ds = objectToInteract as DialogueSequence;
                GlobalCanvasManager.Instance.DialogueHandler.PromptSequence(ds);
            }
            return;
        }
        if (function != null)
        {
            function.Invoke();
            return;
        }
    }
    public bool IsComplete()
    {
        if (!hasInvoked) return false;
        if (wasObjectInvoked)
        {
            if (objectToInteract is DialogueSequence)
            {
                return !GlobalCanvasManager.Instance.DialogueHandler.IsInProgress();
            }
        }
        return true;
    }
}

[System.Serializable]
public class FRAlternateSequence
{
    public string story;
    public int stateTrigger;
    public FRInteraction[] sequence;
}


public class FRInteractable : MonoBehaviour
{

    public bool interactOnTrigger = false;
    public FRInteraction[] interactSequence;
    public List<FRAlternateSequence> alternateSequences;
    private Dictionary<string, FRAlternateSequence> sequenceDictionary;
    public bool playSoundOnInteract = true;
    private void Start()
    {
        GlobalCanvasManager.LoadInstance();
        if (alternateSequences != null)
        {
            sequenceDictionary = new Dictionary<string, FRAlternateSequence>();
            foreach (var alternateSequence in alternateSequences)
            {
                sequenceDictionary.Add(alternateSequence.story, alternateSequence);
            }
        }
    }

    private IEnumerator InteractSequence(FRInteraction[] sequence)
    {
        foreach (var interactableObject in sequence)
        {
            interactableObject.Invoke();
            while (!interactableObject.IsComplete())
            {
                yield return new WaitForEndOfFrame();
            }
        }
    }

    public void OnInteract()
    {
        if (playSoundOnInteract)
            AudioManager.Instance.PlaySFXInScreen("Confirm");

        for (int i = 0; i < GameStoryManager.Instance.ActiveStories.Count; i++)
        {
            var story = GameStoryManager.Instance.ActiveStories[i];
            if (sequenceDictionary.ContainsKey(story.storyName))
            {
                if (story.currentState == sequenceDictionary[story.storyName].stateTrigger)
                {
                    StartCoroutine(InteractSequence(sequenceDictionary[story.storyName].sequence));
                    return;
                }
            }
        }
        if (interactSequence == null) return;
        StartCoroutine(InteractSequence(interactSequence));
    }
}
