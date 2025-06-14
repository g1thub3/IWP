using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DGStartLayer : MenuLayer
{
    bool isFirstPage;
    Transform _buttons1;
    Transform _buttons2;
    Transform _currSelected;
    CanvasGroup _menuGrp;
    public DGStartLayer(Transform buttons1, Transform buttons2, CanvasGroup menuGrp)
    {
        _buttons1 = buttons1;
        _buttons2 = buttons2;
        _menuGrp = menuGrp;
        _currSelected = null;
        isFirstPage = true;
        _buttons2.gameObject.SetActive(false);
    }

    public override void Open()
    {
        if (isFirstPage)
        {
            _currSelected = _buttons1.GetChild(currentSelection);
        } else
        {
            _currSelected = _buttons2.GetChild(currentSelection);
        }
        _buttons1.gameObject.SetActive(isFirstPage);
        _buttons2.gameObject.SetActive(!isFirstPage);
        base.Open();
        Highlight();
        _menuGrp.alpha = 1;
    }
    public override void Close()
    {
        base.Close();
        _menuGrp.alpha = 0;
    }
    private void IncSelection(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        Highlight();
        currentSelection += inc;
        if (isFirstPage)
        {
            if (currentSelection >= 5)
                currentSelection = 0;
            else if (currentSelection < 0)
                currentSelection = 4;
            _currSelected = _buttons1.GetChild(currentSelection);
        }
        else
        {
            if (currentSelection >= 10)
                currentSelection = 5;
            else if (currentSelection < 5)
                currentSelection = 9;
            _currSelected = _buttons2.GetChild(currentSelection - 5);
        }
        Highlight();
    }
    public void SwitchPages()
    {
        if (isFirstPage)
        {
            currentSelection += 5;
        }
        else
        {
            currentSelection -= 5;
        }
        isFirstPage = !isFirstPage;
        _buttons1.gameObject.SetActive(isFirstPage);
        _buttons2.gameObject.SetActive(!isFirstPage);
        IncSelection(0);
    }

    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Up"].WasPressedThisFrame())
            IncSelection(-1);
        if (inputManager.actions["Down"].WasPressedThisFrame())
            IncSelection(1);
    }

    public override void Highlight()
    {
        if (_currSelected == null)
            return;

        var selectionBacking = _currSelected.Find("SelectionBacking");
        var grp = selectionBacking.GetComponent<CanvasGroup>();
        if (grp.alpha == 1)
            grp.alpha = 0;
        else
            grp.alpha = 1;
    }
}
public class DGListLayer : MenuLayer
{
    GameObject _selectionFrame;
    Transform _options;
    GameObject _listPrefab;
    RectTransform _selectionBacking;
    RectTransform _closeMsg;
    List<Transform> addedEntries;
    public DGListLayer(GameObject frame, Transform opt, GameObject prefab, RectTransform closeMsg, RectTransform backing)
    {
        _selectionFrame = frame;
        _options = opt;
        _listPrefab = prefab;
        addedEntries = new List<Transform>();
        _closeMsg = closeMsg;
        _selectionBacking = backing;
    }
    public override void Open()
    {
        base.Open();
        _selectionFrame.SetActive(true);
    }
    public void ClearList()
    {
        addedEntries.Clear();
        for (int i = _options.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(_options.GetChild(i).gameObject);
        }
        _selectionBacking.position = _closeMsg.position;
        _selectionBacking.sizeDelta = _closeMsg.sizeDelta + new Vector2(50, 0);
        if (functions != null)
            functions.Clear();
    }
    public Transform AddEntry()
    {
        var newEntry = Transform.Instantiate(_listPrefab, _options).transform;
        addedEntries.Add(newEntry);
        return newEntry;
    }

    public override void Highlight()
    {
        if (currentSelection < addedEntries.Count)
        {
            var selected = addedEntries[currentSelection];
            _selectionBacking.position = selected.GetComponent<RectTransform>().position;
            _selectionBacking.sizeDelta = selected.GetComponent<RectTransform>().sizeDelta + new Vector2(50, 0);
        }
        else
        {
            _selectionBacking.position = _closeMsg.position;
            _selectionBacking.sizeDelta = _closeMsg.sizeDelta + new Vector2(50, 0);
        }
    }
    public override void OnCreateFrameComplete()
    {
        base.OnCreateFrameComplete();
        Highlight();
    }
    private void IncSelection(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        currentSelection += inc;
        if (currentSelection >= functions.Count)
            currentSelection = 0;
        if (currentSelection < 0)
            currentSelection = functions.Count - 1;
        Highlight();
    }

    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Up"].WasPressedThisFrame())
            IncSelection(-1);
        if (inputManager.actions["Down"].WasPressedThisFrame())
            IncSelection(1);
    }
    public override void Close()
    {
        base.Close();
        _selectionFrame.SetActive(false);
    }

    public override void OnRefresh()
    {
        base.OnRefresh();
        IncSelection(0);
        _selectionFrame.SetActive(true);
    }
}
public class DGDialogueLayer : MenuLayer
{
    GameObject _dialogueFrame;
    Transform _options;
    RectTransform _selectionBacking;

    GameObject _listPrefab;
    List<Transform> addedEntries;
    public DGDialogueLayer(GameObject frame, Transform opt, GameObject prefab, RectTransform backing)
    {
        _dialogueFrame = frame;
        _options = opt;
        _listPrefab = prefab;
        addedEntries = new List<Transform>();
        _selectionBacking = backing;
    }
    public override void Open()
    {
        base.Open();
        _dialogueFrame.SetActive(true);
    }
    public void ClearList()
    {
        addedEntries.Clear();
        for (int i = _options.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(_options.GetChild(i).gameObject);
        }
        if (functions != null)
            functions.Clear();
    }
    public Transform AddEntry()
    {
        var newEntry = Transform.Instantiate(_listPrefab, _options).transform;
        addedEntries.Add(newEntry);
        return newEntry;
    }

    public override void Highlight()
    {
        var selected = addedEntries[currentSelection];
        _selectionBacking.position = selected.GetComponent<RectTransform>().position;
        _selectionBacking.sizeDelta = selected.GetComponent<RectTransform>().sizeDelta + new Vector2(50, 0);
    }
    public override void OnCreateFrameComplete()
    {
        base.OnCreateFrameComplete();
        Highlight();
    }
    private void IncSelection(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        currentSelection += inc;
        if (currentSelection >= functions.Count)
            currentSelection = 0;
        if (currentSelection < 0)
            currentSelection = functions.Count - 1;
        Highlight();
    }

    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Up"].WasPressedThisFrame())
            IncSelection(-1);
        if (inputManager.actions["Down"].WasPressedThisFrame())
            IncSelection(1);
    }
    public override void Close()
    {
        base.Close();
        _dialogueFrame.SetActive(false);
    }

    public override void OnRefresh()
    {
        base.OnRefresh();
        IncSelection(0);
        _dialogueFrame.SetActive(true);
    }
}
public class DGReadLayer : MenuLayer
{
    GameObject _readFrame;

    public DGReadLayer(GameObject readFrame)
    {
        _readFrame = readFrame;
    }
    public override void Open()
    {
        base.Open();
        _readFrame.SetActive(true);
    }
    public override void Highlight()
    {

    }
    public override void Control(PlayerInput inputManager)
    {

    }
    public override void Close()
    {
        base.Close();
        _readFrame.SetActive(false);
    }
}
public class DGPartyLayer : MenuLayer
{
    Transform _partyList;
    public DGPartyLayer(Transform partyList)
    {
        _partyList = partyList;
    }
    public override void Open()
    {
        base.Open();
        Highlight();
    }
    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Up"].WasPressedThisFrame())
        {
            IncSelection(false, false);
        }
        if (inputManager.actions["Right"].WasPressedThisFrame())
        {
            IncSelection(true, true);
        }
        if (inputManager.actions["Down"].WasPressedThisFrame())
        {
            IncSelection(false, true);
        }
        if (inputManager.actions["Left"].WasPressedThisFrame())
        {
            IncSelection(true, false);
        }
    }
    private void IncSelection(bool isHorizontal, bool increase)
    {
        Highlight();
        int inc = increase == true ? 1 : -1;
        if (isHorizontal)
        {
            currentSelection += inc;
            if (currentSelection > 1)
                currentSelection = 0;
            else if (currentSelection < 0)
                currentSelection = 1;
        }
        else
        {
            inc *= 2;
            currentSelection += inc;
            if (currentSelection > functions.Count - 1)
                currentSelection %= 2;
            else if (currentSelection < 0)
                currentSelection = (currentSelection % 2) * -1;
        }
        Highlight();
    }

    public override void Highlight()
    {
        var child = _partyList.GetChild(currentSelection);
        var selection = child.Find("Selection").GetComponent<CanvasGroup>();
        selection.alpha = selection.alpha == 1 ? 0 : 1;
    }
    public override void OnRefresh()
    {
        base.OnRefresh();
        Highlight();
    }
}


public class DungeonMenuHandler : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] private Transform _partyList;
    [SerializeField] private GameObject _partyEntry;
    [SerializeField] private Transform _btns1, _btns2;

    [SerializeField] private GameObject _sWindow;
    [SerializeField] private TMP_Text _sWindowTitle;
    [SerializeField] private GameObject _dialogueOptions;
    [SerializeField] private RectTransform _closeMsg, _selectionBacking;
    [SerializeField] private Transform _itemList, _optionsContainer;
    [SerializeField] private GameObject _itemEntry;

    [SerializeField] private GameObject _textDisplay;
    [SerializeField] private TMP_Text _readTitle, _readContent;

    private DGGenerator _dungeonGen;
    private DGGameManager _gameManager;
    private PlayerInput _playerInput;
    private List<MenuLayer> _layers;
    private List<StaticMenuFunction> _helpFunctions;
    private MenuLayer CurrentLayer
    {
        get
        {
            if (_layers.Count < 1)
                return null;
            else
                return _layers.Last();
        }
    }
    private void Start()
    {
        GlobalCanvasManager.LoadInstance();
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _gameManager = FindAnyObjectByType<DGGameManager>();
        _playerInput = FindAnyObjectByType<PlayerInput>();
        _layers = new List<MenuLayer>();
        _helpFunctions = new List<StaticMenuFunction>();
        _helpFunctions.Add(new StaticMenuFunction("See Active Quests", SeeQuests));
        _helpFunctions.Add(new StaticMenuFunction("Controls", delegate
        {
            CreateReadLayer("Controls", "Arrow Keys: Up, Down, Left, Right\n\nZ: Accept/Interact\nX: Decline/Open Menu");
        }));
        _helpFunctions.Add(new StaticMenuFunction("Dungeon Guide", delegate
        {
            CreateReadLayer("Dungeon Guide", "Find the staircase to go to the next floor! Keep going through floors to reach the end of the dungeon. " +
                "If you lose or escape the dungeon, you will lose held items, half your gold and half your inventory. Your quests will not be completed as well.");
        }));
        _helpFunctions.Add(new StaticMenuFunction("Doing Quests", delegate
        {
            CreateReadLayer("Doing Quests", "Once you have completed your quest objective, you will be given the option to leave. You can continue exploring the dungeon or doing other quests. " +
                "When a quest is labelled in yellow, that means your quest target is in the floor you're on. When a quest is labelled in green, the quest has been completed." +
                "If you lose or escape, you won't get the rewards for any quest.");
        }));
    }
    private void SeeQuests()
    {
        var questLayer = new DGListLayer(_sWindow, _itemList, _itemEntry, _closeMsg, _selectionBacking);
        questLayer.refresh = delegate {
            _sWindowTitle.text = "Quests";
            questLayer.ClearList();
            questLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = 0; i < _gameManager.ActiveQuests.Count; i++)
            {
                // OPEN ITEM //
                var q = _gameManager.ActiveQuests[i];
                var item = questLayer.AddEntry();
                TMP_Text textcomp = item.GetComponent<TMP_Text>();
                string label = "(" + (!GlobalGameManager.Instance.selectedDungeon.isAscending ? "B" : string.Empty) + q.quest.floor + "F) " + q.QuestObjectiveText;
                if (label.Length > 25)
                {
                    label = label.Substring(0, 25) + "...";
                }
                textcomp.text = label;
                if (_gameManager.CurrentFloor == q.quest.floor)
                {
                    textcomp.color = Color.yellow;
                }
                if (q.quest.questCompleted)
                {
                    textcomp.color = Color.green;
                }

                questLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DGDialogueLayer(_dialogueOptions, _optionsContainer, _itemEntry, _selectionBacking);
                    dialogueLayer.refresh = delegate {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();

                        var info = dialogueLayer.AddEntry();
                        info.GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate {
                            string desc = string.Format("Title: {0}\n\n{1}\n{2}\n{3}\n{4}\n{5}", q.QuestTitleText, q.QuestClientText,
                                q.QuestPlaceText, q.QuestObjectiveText, q.QuestDifficultyText, q.QuestRewardText);
                            CreateReadLayer("About: " + q.clientName + "'s Quest", desc);
                        });

                        var close = dialogueLayer.AddEntry();
                        close.GetComponent<TMP_Text>().text = "Close";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    dialogueLayer.Open();
                });
            }
            questLayer.functions.Add(questLayer.Close);
        };
        _layers.Add(questLayer);
        questLayer.Open();
    }

    private void CreateReadLayer(string title, string content)
    {
        var newLayer = new DGReadLayer(_textDisplay);
        newLayer.refresh = delegate
        {
            _readTitle.text = title;
            _readContent.text = content;
            newLayer.functions = new List<MenuLayer.MenuFunction>();
            newLayer.functions.Add(newLayer.Close);
        };
        _layers.Add(newLayer);
        newLayer.Open();
    }

    private void OpenMoves()
    {
        CreateReadLayer("None", "Feature not implemented yet.");
    }
    private void OpenInventory()
    {
        var inventoryLayer = new DGListLayer(_sWindow, _itemList, _itemEntry, _closeMsg, _selectionBacking);
        inventoryLayer.refresh = delegate {
            _sWindowTitle.text = "Inventory";
            inventoryLayer.ClearList();
            inventoryLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = 0; i < GlobalGameManager.Instance.inventory.Count; i++)
            {
                // OPEN ITEM //
                var invItem = GlobalGameManager.Instance.inventory[i];
                var item = inventoryLayer.AddEntry();
                item.GetComponent<TMP_Text>().text = invItem.module.itemName + (invItem.IsQuestTarget ? " (Quest)" : string.Empty);
                inventoryLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DGDialogueLayer(_dialogueOptions, _optionsContainer, _itemEntry, _selectionBacking);
                    dialogueLayer.refresh = delegate {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();

                        if (!invItem.IsQuestTarget)
                        {
                            var use = dialogueLayer.AddEntry();
                            use.GetComponent<TMP_Text>().text = "Use";
                            dialogueLayer.functions.Add(delegate {
                                var partyLayer = new DGPartyLayer(_partyList);
                                partyLayer.refresh = delegate
                                {
                                    partyLayer.functions = new List<MenuLayer.MenuFunction>();
                                    for (int j = 0; j < _dungeonGen.ActiveParty.Count; j++)
                                    {
                                        partyLayer.functions.Add(delegate
                                        {
                                            GlobalGameManager.Instance.UseItem(inventoryLayer.CurrentSelection, _dungeonGen.ActiveParty[partyLayer.CurrentSelection]);
                                            _gameManager.TurnCompleted.Invoke();
                                            partyLayer.Close();
                                            dialogueLayer.Close();
                                        });
                                    }
                                };
                                _layers.Add(partyLayer);
                                partyLayer.Open();
                            });

                            var hold = dialogueLayer.AddEntry();
                            hold.GetComponent<TMP_Text>().text = "Hold";
                            dialogueLayer.functions.Add(delegate {
                                var partyLayer = new DGPartyLayer(_partyList);
                                partyLayer.refresh = delegate
                                {
                                    partyLayer.functions = new List<MenuLayer.MenuFunction>();
                                    for (int j = 0; j < _dungeonGen.ActiveParty.Count; j++)
                                    {
                                        partyLayer.functions.Add(delegate
                                        {
                                            GlobalGameManager.Instance.HoldItem(inventoryLayer.CurrentSelection, partyLayer.CurrentSelection);
                                            partyLayer.Close();
                                            dialogueLayer.Close();
                                        });
                                    }
                                };
                                _layers.Add(partyLayer);
                                partyLayer.Open();
                            });
                        }

                        var info = dialogueLayer.AddEntry();
                        info.GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate {
                            CreateReadLayer("About: " + invItem.module.itemName, invItem.module.itemDescription);
                        });

                        if (!invItem.IsQuestTarget)
                        {
                            var drop = dialogueLayer.AddEntry();
                            drop.GetComponent<TMP_Text>().text = "Drop";
                            dialogueLayer.functions.Add(delegate {
                                DGPlayer plr = FindAnyObjectByType<DGPlayer>();
                                if (plr != null)
                                {
                                    plr.GetComponent<CharacterBehaviour>().DropItem(inventoryLayer.CurrentSelection);
                                }
                                dialogueLayer.Close();
                            });

                            var trash = dialogueLayer.AddEntry();
                            trash.GetComponent<TMP_Text>().text = "Trash";
                            dialogueLayer.functions.Add(delegate {
                                GlobalGameManager.Instance.inventory.RemoveAt(inventoryLayer.CurrentSelection);
                                dialogueLayer.Close();
                            });
                        }

                        var close = dialogueLayer.AddEntry();
                        close.GetComponent<TMP_Text>().text = "Close";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    dialogueLayer.Open();
                });
            }
            inventoryLayer.functions.Add(inventoryLayer.Close);
        };
        _layers.Add(inventoryLayer);
        inventoryLayer.Open();
    }

    private void OpenParty()
    {
        var newLayer = new DGPartyLayer(_partyList);
        newLayer.refresh = delegate
        {
            newLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = 0; i < _dungeonGen.ActiveParty.Count; i++)
            {
                var member = _dungeonGen.ActiveParty[newLayer.CurrentSelection];
                var c = member.character;
                newLayer.functions.Add(delegate
                {
                    newLayer.Highlight();
                    CreateReadLayer("About: " + c.Profile.characterName, member.GetDescription());
                });
            }
        };
        _layers.Add(newLayer);
        newLayer.Open();
    }
    private void CheckGround()
    {
        //CloseCurrentWindow();
        DGPlayer plr = FindAnyObjectByType<DGPlayer>();
        if (plr != null)
        {
            TileInfo currentTile = plr.Floor.CoordToTileInfo(plr.Position);
            if (currentTile.item != null)
            {
                currentTile.item.OnInteract(plr, plr.Floor);
            }
            else if (currentTile.structure != null)
            {
                currentTile.structure.OnInteract(plr, plr.Floor);
            }
        }
    }
    private void OpenHelp()
    {
        var helpLayer = new DGListLayer(_sWindow, _itemList, _itemEntry, _closeMsg, _selectionBacking);
        helpLayer.refresh = delegate {
            _sWindowTitle.text = "Help";
            helpLayer.ClearList();
            helpLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = 0; i < _helpFunctions.Count; i++)
            {
                // OPEN ITEM //
                var option = _helpFunctions[i];
                var item = helpLayer.AddEntry();
                item.GetComponent<TMP_Text>().text = option.title;
                helpLayer.functions.Add(option.function);
            }
            helpLayer.functions.Add(helpLayer.Close);
        };
        _layers.Add(helpLayer);
        helpLayer.Open();
    }
    private void Quicksave()
    {
        CreateReadLayer("None", "Feature not implemented yet.");
    }

    private IEnumerator WaitForAnswer()
    {
        while (GlobalCanvasManager.Instance.PromptHandler.IsPromptInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        if (GlobalCanvasManager.Instance.PromptHandler.TakeAnswer() == 0)
        {
            _gameManager.OnEscape.Invoke();
        }
    }

    private void Escape()
    {
        PromptInfo prompt = new PromptInfo();
        prompt.message = "Are you sure you want to escape the dungeon? (This will count as a loss and you will lose your items and gold)";
        prompt.options = new string[2];
        prompt.options[0] = "Yes";
        prompt.options[1] = "No";
        GlobalCanvasManager.Instance.PromptHandler.Prompt(prompt);
        StartCoroutine(WaitForAnswer());

    }
    public void LoadParty()
    {
        for (int i = _partyList.childCount - 1; i >= 0; i--) { 
            Destroy(_partyList.GetChild(i).gameObject);
        }
        for (int i = 0; i < _dungeonGen.ActiveParty.Count; i++)
        {
            var cb = _dungeonGen.ActiveParty[i];
            var c = cb.character;
            var newEntry = Instantiate(_partyEntry, _partyList);
            newEntry.transform.Find("CharacterName").GetComponent<TMP_Text>().text = c.Profile.characterName;
            newEntry.transform.Find("CharacterLevel").GetComponent<TMP_Text>().text = "Lv. " + c.characterLevel;
            newEntry.transform.Find("CharacterExp").GetComponent<TMP_Text>().text = "EXP: " + c.experiencePoints + "/" + c.ExpToNextLevel;
            newEntry.transform.Find("HP").GetComponent<TMP_Text>().text = "HP: " + cb.health + "/" + c.maxHealth.CurrStat;
            newEntry.transform.Find("Energy").GetComponent<TMP_Text>().text = "EN: " + cb.energy + "/" + c.maxEnergy.CurrStat;
            newEntry.transform.Find("Mana").GetComponent<TMP_Text>().text = "MN: " + cb.mana + "/" + c.maxMana.CurrStat;
            newEntry.transform.Find("Hunger").GetComponent<TMP_Text>().text = "HG: " + cb.hunger + "/" + c.hungerSize.CurrStat;
        }
    }

    public bool IsOpen
    {
        get { return GetComponent<CanvasGroup>().alpha > 0; }
    }

    private void CreateMain()
    {
        LoadParty();
        var newLayer = new DGStartLayer(_btns1, _btns2, GetComponent<CanvasGroup>());
        newLayer.refresh = delegate
        {
            newLayer.functions = new List<MenuLayer.MenuFunction>();
            newLayer.functions.Add(OpenMoves);
            newLayer.functions.Add(OpenInventory);
            newLayer.functions.Add(OpenParty);
            newLayer.functions.Add(newLayer.SwitchPages);
            newLayer.functions.Add(newLayer.Close);
            newLayer.functions.Add(CheckGround);
            newLayer.functions.Add(OpenHelp);
            newLayer.functions.Add(Quicksave);
            newLayer.functions.Add(Escape);
            newLayer.functions.Add(newLayer.SwitchPages);
        };
        _layers.Add(newLayer);
        CurrentLayer.Open();
    }

    private void PerformFunction()
    {
        if (CurrentLayer != null)
        {
            CurrentLayer.functions[CurrentLayer.CurrentSelection]();
        }
    }

    private void Update()
    {
        if (GlobalCanvasManager.Instance.PromptHandler.IsPromptInProgress || !_gameManager.IsGameActive) return;
        if (CurrentLayer != null)
        {
            if (CurrentLayer.nextFrameTrigger)
            {
                CurrentLayer.OnCreateFrameComplete();
                CurrentLayer.nextFrameTrigger = false;
            }
            CurrentLayer.Control(_playerInput);
            if (_playerInput.actions["Accept"].WasPressedThisFrame())
            {
                PerformFunction();
            }
            if (_playerInput.actions["Decline"].WasPressedThisFrame())
            {
                CurrentLayer.Close();
            }
            if (!CurrentLayer.IsOpen)
            {
                _layers.Remove(CurrentLayer);
                if (CurrentLayer != null)
                {
                    if (CurrentLayer.refresh != null)
                    {
                        CurrentLayer.refresh();
                        CurrentLayer.OnRefresh();
                    }
                }
            }
        }
        else
        {
            if (_playerInput.actions["Decline"].WasPressedThisFrame())
            {
                DGPlayer controller = FindAnyObjectByType<DGPlayer>();
                if (controller != null)
                {
                    if (controller.CanControl)
                        CreateMain();
                }
            }
        }
    }
}
