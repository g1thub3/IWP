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

    public LevelUpHandler LevelUpHandler
    {
        get { return GetComponent<LevelUpHandler>(); }
    }

    public bool IsInteractionActive
    {   
        get { return PromptHandler.IsInProgress() || DialogueHandler.IsInProgress() || LevelUpHandler.IsInProgress() || FreeRoamMenuHandler.IsOpen || CutsceneManager.Instance.IsInProgress(); }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
}
