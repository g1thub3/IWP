using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenuLayer : MenuLayer
{
    Transform _buttons;
    Transform _currSelected;

    public MainMenuLayer(Transform buttons)
    {
        _buttons = buttons;
        _currSelected = null;
    }
    public override void Open()
    {
        _currSelected = _buttons.GetChild(CurrentSelection);
        base.Open();
        Highlight();
    }
    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Right"].WasPressedThisFrame())
            IncSelection(1);
        if (inputManager.actions["Left"].WasPressedThisFrame())
            IncSelection(-1);
    }

    public override void Highlight()
    {
        if (_currSelected != null)
        {
            bool isEnabled = _currSelected.Find("SelectionBacking").GetComponent<Image>().enabled;
            _currSelected.Find("SelectionBacking").GetComponent<Image>().enabled = !isEnabled;
        }
    }
    public override void Close()
    {

    }
    private void IncSelection(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        Highlight();
        if (CurrentSelection + inc >= functions.Count)
            CurrentSelection = 0;
        else if (CurrentSelection + inc < 0)
            CurrentSelection = functions.Count - 1;
        else
            CurrentSelection += inc;
        _currSelected = _buttons.GetChild(currentSelection);
        Highlight();
    }
}

public class MainMenuHandler : LayeredUI
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] Transform _menuButtons;
    [SerializeField] CanvasGroup _continueGrp;
    private bool _fileFound;
    SettingsHandler _settings;
    private new void Start()
    {
        base.Start();
        _fileFound = SaveDataManager.Instance.QuickSaveFile != null || SaveDataManager.Instance.BaseFile != null;
        if (_fileFound)
        {
            _continueGrp.alpha = 1.0f;
        } else
        {
            _continueGrp.alpha = 0.5f;
        }
        AudioManager.Instance.PlayBGM("MainMenu", 0.0f);
        GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = false;
        _settings = GlobalCanvasManager.Instance.SettingsHandler;
        _inputManager = GetComponent<PlayerInput>();
        CreateMain();
    }

    private void CreateMain()
    {
        var layer = new MainMenuLayer(_menuButtons);
        layer.selectionChanged = delegate
        {

        };
        layer.refresh = delegate
        {
            layer.functions = new List<MenuLayer.MenuFunction>();
            layer.functions.Add(delegate
            {

                if (DebugTools.Instance.StoryDebugOn)
                {
                    GameStoryManager.Instance.BeginStory("Test");
                } else
                {
                    GameStoryManager.Instance.BeginStory("Main Story");
                }
                GlobalGameManager.Instance.CycleDay();
                GameSceneManager.Instance.ToDorm();
            });
            layer.functions.Add(delegate
            {
                if (_fileFound)
                {
                    GlobalCanvasManager.Instance.SaveDataUIHandler.LoadMenu();
                }
            });
            layer.functions.Add(delegate
            {
                _settings.Open();
            });
            layer.functions.Add(delegate
            {
                GlobalCanvasManager.Instance.PauseHandler.Open();
            });
        };
        _layers.Add(layer);
        CurrentLayer.Open();
    }

    private void Update()
    {
        Process();
    }
}
