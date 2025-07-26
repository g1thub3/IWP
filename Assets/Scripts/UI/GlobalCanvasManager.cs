using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalCanvasManager : SingletonMonobehaviour<GlobalCanvasManager>
{
    public AudioSource SFXSource, BGMSource;
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

    public void PrintInteractions()
    {
        Debug.Log(string.Format("Prompt: {0} | Dialogue: {1} | Level Up: {2} | FreeRoam: {3} | Cutscene: {4}", 
            PromptHandler.IsInProgress(), DialogueHandler.IsInProgress(), LevelUpHandler.IsInProgress(), FreeRoamMenuHandler.IsOpen, CutsceneManager.Instance.IsInProgress()));
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
