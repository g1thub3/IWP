using System.Collections;
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
                return !GlobalCanvasManager.Instance.DialogueHandler.IsSequenceRunning;
            }
        }
        return true;
    }
}


public class FRInteractable : MonoBehaviour
{

    public bool interactOnTrigger = false;
    public FRInteraction[] interactSequence;

    private void Start()
    {
        GlobalCanvasManager.LoadInstance();
    }

    private IEnumerator InteractSequence()
    {
        foreach (var interactableObject in interactSequence)
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
        if (interactSequence == null) return;
        StartCoroutine(InteractSequence());
    }
}
