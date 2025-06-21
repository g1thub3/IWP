using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalCanvasManager : SingletonMonobehaviour<GlobalCanvasManager>
{
    public PlayerInput GlobalInput
    {
        get { return GetComponent<PlayerInput>(); }
    }
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
