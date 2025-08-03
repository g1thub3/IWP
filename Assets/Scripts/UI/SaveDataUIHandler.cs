using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LoadDataLayer : MenuLayer
{
    GameObject _baseFrame, _loadFrame;
    Image _selectionCancel, _selectionContinue;

    public LoadDataLayer(GameObject baseFrame, GameObject loadFrame, Image selectionCancel, Image selectionContinue)
    {
        _baseFrame = baseFrame;
        _loadFrame = loadFrame;
        _selectionCancel = selectionCancel;
        _selectionContinue = selectionContinue;
    }

    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Left"].WasPressedThisFrame() || inputManager.actions["Right"].WasPressedThisFrame())
        {
            if (CurrentSelection == 0)
                CurrentSelection = 1;
            else
                CurrentSelection = 0;
            Highlight();
        }
    }

    public override void Highlight()
    {
        if (CurrentSelection == 0)
        {
            _selectionCancel.enabled = true;
            _selectionContinue.enabled = false;
        } else
        {
            _selectionCancel.enabled = false;
            _selectionContinue.enabled = true;
        }
    }

    public override void Open()
    {
        base.Open();
        _baseFrame.SetActive(true);
        _loadFrame.SetActive(true);
    }
    public override void Close()
    {
        base.Close();
        _baseFrame.SetActive(false);
        _loadFrame.SetActive(false);
    }
}

public class SaveDataUIHandler : LayeredUI
{
    [Header("Base")]
    [SerializeField] GameObject _frame;
    [SerializeField] GameObject _loadFrame, _saveFrame;

    [Header("Load")]
    [SerializeField] Image _selectionCancel;
    [SerializeField] Image _selectionContinue;
    [SerializeField] GameObject _dungeonInfo, _loadData, _noData;
    [SerializeField] TMP_Text _leaderLvlLoad, _walletGoldLoad, _inventoryLoad, _playTimeLoad, _locationLoad, _bankGoldLoad, _storageLoad, _dgFloorLoad, _dgStatsLoad;

    [Header("Save Menu")]
    [SerializeField] Image _selectionCancel2;
    [SerializeField] Image _selectionOverwrite;
    [SerializeField] TMP_Text _overwriteText;
    [SerializeField] Image _cooldownImg;

    [Header("Previous Data")]
    [SerializeField] GameObject _dungeonInfoPrev; 
    [SerializeField] GameObject _prevData, _noPrevData;
    [SerializeField] TMP_Text _leaderLvlPrev, _walletGoldPrev, _inventoryPrev, _playTimePrev, _locationPrev, _bankGoldPrev, _storagePrev, _dgFloorPrev, _dgStatsPrev;

    [Header("Current Data")]
    [SerializeField] GameObject _dungeonInfoCurr;
    [SerializeField] TMP_Text _leaderLvlCurr, _walletGoldCurr, _inventoryCurr, _playTimeCurr, _locationCurr, _bankGoldCurr, _storageCurr, _dgFloorCurr, _dgStatsCurr;

    private GameSaveData _selectedFile;
    private GameSaveData _currData;

    public bool IsOpen
    {
        get { return _frame.activeSelf; }
    }

    PromptInfo _savePrompt;
    PromptInfo _quicksavePrompt;
    DialogueData[] _saveSequence;

    private bool _goToMainMenu;
    [SerializeField] private float _overwriteCooldown = 1.5f;
    private float _overwriteTime;

    private new void Start()
    {
        base.Start();
        _inputManager = GetComponent<PlayerInput>();
        _overwriteTime = 0;

        _saveSequence = new DialogueData[2] {new DialogueData(), new DialogueData()};
        _saveSequence[0].isSpriteShowing = false;
        _saveSequence[0].textSpeed = 0.025f;
        _saveSequence[0].waitTime = 3.5f;
        _saveSequence[0].canSkip = false;
        _saveSequence[0].autoNext = true;
        _saveSequence[0].content = new string[] { "Saving game...\nPlease avoid closing the game during saving..." };

        _saveSequence[1].isSpriteShowing = false;
        _saveSequence[1].textSpeed = 0.025f;
        _saveSequence[1].waitTime = 3.5f;
        _saveSequence[1].canSkip = true;
        _saveSequence[1].autoNext = false;
        _saveSequence[1].content = new string[] { "Game saved!" };

        _savePrompt = PromptInfo.New("What would you like to do?", new string[]
        {
            "Back",
            "Save the game.",
            "Save the game and return to the main menu."
        }, new PromptInfo.OptionFunction[]
        {
            PromptInfo.NullFunction,
            delegate
            {
                _goToMainMenu = false;
                SaveMenu();
            },
            delegate
            {
                _goToMainMenu = true;
                SaveMenu();
            }
        });
    }

    private IEnumerator WaitForSaveEnd()
    {
        while (GlobalCanvasManager.Instance.IsInteractionActive)
            yield return new WaitForEndOfFrame();
        GameSceneManager.Instance.ToMainMenu();
    }

    public void SavePrompt()
    {
        GlobalCanvasManager.Instance.PromptHandler.Prompt(_savePrompt);
    }

    private string TimeToString(int amt)
    {
        string newString = string.Empty;

        int minutes = (int)Mathf.Floor((float)amt / 60);
        int seconds = amt - (minutes * 60);
        int hours = (int)Mathf.Floor((float)minutes / 60);
        minutes -= hours * 60;

        newString = string.Format("{0}:{1}:{2}", hours.ToString("00"), minutes.ToString("00"), seconds.ToString("00"));
        return newString;
    }

    public void LoadMenu()
    {
        var layer = new LoadDataLayer(_frame, _loadFrame, _selectionCancel, _selectionContinue);
        layer.refresh = delegate
        {
            _selectedFile = SaveDataManager.Instance.QuickSaveFile != null ? SaveDataManager.Instance.QuickSaveFile : SaveDataManager.Instance.BaseFile;
            if (_selectedFile != null)
            {
                _loadData.SetActive(true);
                _noData.SetActive(false);
                _leaderLvlLoad.text = "Lv. " + _selectedFile.party[0].level;
                _walletGoldLoad.text = "Gold in Wallet: " + _selectedFile.walletGold;
                _inventoryLoad.text = "Items in Inventory: " + _selectedFile.inventoryItemKeys.Count;
                _playTimeLoad.text = "Play Time: " + TimeToString(_selectedFile.playTime);
                _locationLoad.text = "Location: " + _selectedFile.locationName;
                _bankGoldLoad.text = "Gold in Bank: " + _selectedFile.bankGold;
                _storageLoad.text = "Items in Storage: " + _selectedFile.storageItemKeys.Count;
                if (_selectedFile.dungeonSave != null)
                {
                    _dungeonInfo.SetActive(true);
                } else
                {
                    _dungeonInfo.SetActive(false);
                }
            } else
            {
                _loadData.SetActive(false);
                _noData.SetActive(true);
            }

                layer.functions = new List<MenuLayer.MenuFunction>();
            layer.functions.Add(delegate
            {
                layer.Close();
            });
            layer.functions.Add(delegate
            {
                if (_selectedFile == null)
                    return;
                _selectedFile.LoadGameSave();
                layer.Close();
                GameSceneManager.Instance.ToSavedScene(_selectedFile.sceneName);
            });
        };
        _layers.Add(layer);
        layer.Open();
    }

    public void SaveMenu()
    {
        var layer = new LoadDataLayer(_frame, _saveFrame, _selectionCancel2, _selectionOverwrite);
        layer.refresh = delegate
        {
            _overwriteTime = _overwriteCooldown;
            _overwriteText.text = "...";
            _cooldownImg.fillAmount = 1.0f;

            _selectedFile = SaveDataManager.Instance.QuickSaveFile != null ? SaveDataManager.Instance.QuickSaveFile : SaveDataManager.Instance.BaseFile;
            if (_selectedFile != null)
            {
                _prevData.SetActive(true);
                _noPrevData.SetActive(false);
                _leaderLvlPrev.text = "Lv. " + _selectedFile.party[0].level;
                _walletGoldPrev.text = "Gold in Wallet: " + _selectedFile.walletGold;
                _inventoryPrev.text = "Items in Inventory: " + _selectedFile.inventoryItemKeys.Count;
                _playTimePrev.text = "Play Time: " + TimeToString(_selectedFile.playTime);
                _locationPrev.text = "Location: " + _selectedFile.locationName;
                _bankGoldPrev.text = "Gold in Bank: " + _selectedFile.bankGold;
                _storagePrev.text = "Items in Storage: " + _selectedFile.storageItemKeys.Count;
                if (_selectedFile.dungeonSave != null)
                {
                    _dungeonInfo.SetActive(true);
                }
                else
                {
                    _dungeonInfo.SetActive(false);
                }
            }
            else
            {
                _prevData.SetActive(false);
                _noPrevData.SetActive(true);
            }

            _currData = GameSaveData.Construct();
            _leaderLvlCurr.text = "Lv. " + _currData.party[0].level;
            _walletGoldCurr.text = "Gold in Wallet: " + _currData.walletGold;
            _inventoryCurr.text = "Items in Inventory: " + _currData.inventoryItemKeys.Count;
            _playTimeCurr.text = "Play Time: " + TimeToString(_currData.playTime);
            _locationCurr.text = "Location: " + _currData.locationName;
            _bankGoldCurr.text = "Gold in Bank: " + _currData.bankGold;
            _storageCurr.text = "Items in Storage: " + _currData.storageItemKeys.Count;
            if (_currData.dungeonSave != null)
            {
                _dungeonInfo.SetActive(true);
            }
            else
            {
                _dungeonInfo.SetActive(false);
            }

            layer.functions = new List<MenuLayer.MenuFunction>();
            layer.functions.Add(delegate
            {
                layer.Close();
            });
            layer.functions.Add(delegate
            {
                if (_overwriteTime > 0.0f)
                    return;
                layer.Close();
                SaveDataManager.Instance.WriteSaveData();
                GlobalCanvasManager.Instance.DialogueHandler.PromptSequence(_saveSequence);
                if (_goToMainMenu)
                {
                    StartCoroutine(WaitForSaveEnd());
                }
            });
        };
        _layers.Add(layer);
        layer.Open();
    }

    private void Update()
    {
        Process();
        if (_overwriteTime > 0.0f && _saveFrame.activeSelf)
        {
            _overwriteTime -= Time.deltaTime;
            _cooldownImg.fillAmount = _overwriteTime / _overwriteCooldown;
            if (_overwriteTime <= 0.0f)
            {
                _overwriteText.text = "Overwrite";
            }
        }
    }
}
