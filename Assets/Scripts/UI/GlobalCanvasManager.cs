using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GlobalCanvasManager : SingletonMonobehaviour<GlobalCanvasManager>, IYieldable
{
    [SerializeField] CanvasGroup _globalTransGrp;
    public RectTransform QuestReminder;
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

    public SettingsHandler SettingsHandler
    {
        get { return GetComponent<SettingsHandler>(); }
    }
    public PauseHandler PauseHandler
    {
        get { return GetComponent<PauseHandler>(); }
    }

    public SaveDataUIHandler SaveDataUIHandler
    {
        get { return GetComponent<SaveDataUIHandler>(); }
    }

    public bool IsInteractionActive
    {   
        get { return IsInProgress() || PromptHandler.IsInProgress() || DialogueHandler.IsInProgress() || LevelUpHandler.IsInProgress() || CutsceneManager.Instance.IsInProgress() || SettingsHandler.IsOpen || PauseHandler.IsOpen; }
    }

    public bool IsInteractionActiveFreeRoam
    {
        get { return IsInteractionActive || FreeRoamMenuHandler.IsOpen || SaveDataUIHandler.IsOpen; }
    }

    public void PrintInteractions()
    {
        Debug.Log(string.Format("Prompt: {0} | Dialogue: {1} | Level Up: {2} | FreeRoam: {3} | Cutscene: {4} | Settings: {5} | Transition: {6} | Save: {7}", 
            PromptHandler.IsInProgress(), DialogueHandler.IsInProgress(), LevelUpHandler.IsInProgress(), FreeRoamMenuHandler.IsOpen, CutsceneManager.Instance.IsInProgress(), SettingsHandler.IsOpen, IsInProgress(), SaveDataUIHandler.IsOpen));
    }

    private void Start()
    {
        _transitionCounter = 0;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseHandler.Open();
        }
        if (CutsceneManager.Instance.IsInProgress())
        {
            if (GlobalInput.actions["Accept"].WasPressedThisFrame() || GlobalInput.actions["Skip"].IsPressed())
                CutsceneManager.Instance.CutsceneSkipInput();
        }
        if (SceneManager.GetActiveScene().name != "MainMenuScene" && SceneManager.GetActiveScene().name != "SplashScreenScene")
        {
            GlobalGameManager.Instance.playTime += Time.deltaTime;
        }
    }

    private int _transitionCounter;
    private bool _transitionInProgress;
    private bool _transitionSkip;
    public void SkipTransition() => _transitionSkip = true;
    public void FadeTransition(float time, Action OnComplete = null, bool transIn = true, bool autoFadeOut = true)
    {
        _transitionCounter++;
        _transitionInProgress = true;
        _transitionSkip = false;
        StartCoroutine(FadeTransitionCoroutine(time, transIn, autoFadeOut, OnComplete));
    }

    private IEnumerator FadeTransitionCoroutine(float time, bool transIn, bool autoFadeOut, Action OnComplete)
    {
        int currCounter = _transitionCounter;
        Transition newTrans =  new Transition();
        newTrans.max = time;
        while (newTrans.Progression < 1.0f)
        {
            if (currCounter != _transitionCounter)
                break;
            newTrans.Progress();
            if (transIn)
            {
                _globalTransGrp.alpha = newTrans.Progression;
            } else
            {
                _globalTransGrp.alpha = 1 - newTrans.Progression;
            }
            if (!_transitionSkip)
                yield return new WaitForEndOfFrame();
        }
        _transitionSkip = false;
        _transitionInProgress = false;
        if (OnComplete != null)
        {
            OnComplete.Invoke();
        }
        if (autoFadeOut)
        {
            while (newTrans.Progression > 0.0f)
            {
                if (currCounter != _transitionCounter)
                    break;
                newTrans.Revert();
                if (transIn)
                {
                    _globalTransGrp.alpha = newTrans.Progression;
                }
                else
                {
                    _globalTransGrp.alpha = 1 - newTrans.Progression;
                }
                if (!_transitionSkip)
                    yield return new WaitForEndOfFrame();
            }
        }
    }
    public bool IsInProgress()
    {
        return _transitionInProgress;
    }

    public void RemindQuest()
    {
        QuestReminder.anchoredPosition = new Vector2(0, 150);
        QuestReminder.gameObject.SetActive(true);
        StartCoroutine(ReminderCoroutine());
    }
    private IEnumerator ReminderCoroutine()
    {
        Transition newTrans = new Transition();
        newTrans.max = 0.5f;
        while (newTrans.Progression < 1.0f)
        {
            if (!QuestReminder.gameObject.activeSelf)
                break;
            QuestReminder.anchoredPosition = new Vector2(0,150 * (1 - newTrans.Progression));
            newTrans.Progress();
            yield return new WaitForEndOfFrame();
        }
    }
}
