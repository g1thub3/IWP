using UnityEngine;

public class GlobalCanvasManager : SingletonMonobehaviour<GlobalCanvasManager>
{
    public DialogueHandler DialogueHandler {
        get { return GetComponent<DialogueHandler>(); }
    }

    public PromptHandler PromptHandler { 
        get { return GetComponent<PromptHandler>(); } 
    }

    public FreeRoamMenuHandler FreeRoamMenuHandler
    {
        get { return GetComponent<FreeRoamMenuHandler>(); }
    }
}
