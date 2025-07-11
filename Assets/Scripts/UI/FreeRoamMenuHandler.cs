using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public struct StaticMenuFunction
{
    public string title;
    public MenuLayer.MenuFunction function;
    public StaticMenuFunction(string t, MenuLayer.MenuFunction f)
    {
        title = t;
        function = f;
    }
}

public abstract class MenuLayer {
    protected bool isOpen;
    protected int currentSelection;
    public int CurrentSelection
    {
        get { return currentSelection; }
    }
    public bool IsOpen { get { return isOpen; } }
    public delegate void MenuFunction();
    public List<MenuFunction> functions;
    public MenuFunction refresh;
    public virtual void Open()
    {
        isOpen = true;
        currentSelection = 0;
        if (refresh != null)
            refresh();
        nextFrameTrigger = true;
    }
    public abstract void Highlight();
    public abstract void Control(PlayerInput inputManager);
    public virtual void Close() {
        isOpen = false;
        Highlight();
    }

    public virtual void OnRefresh()
    {
        nextFrameTrigger = true;
    }
    public bool nextFrameTrigger;

    [Tooltip("When the frame the layer has been created is completed, run this function (for post-frame UI calculations).")]
    public virtual void OnCreateFrameComplete()
    {

    }
}

public class StartLayer : MenuLayer
{
    Transform _buttons;
    Transform _currSelected;
    CanvasGroup _menuGrp;
    public StartLayer(Transform buttons, CanvasGroup menuGrp)
    {
        _buttons = buttons;
        _menuGrp = menuGrp;
        _currSelected = null;
    }
    public override void Open()
    {
        _currSelected = _buttons.GetChild(currentSelection);
        base.Open();
        Highlight();
        _menuGrp.alpha = 1;
    }
    public override void Highlight()
    {
        if (_currSelected != null)
        {
            bool isEnabled = _currSelected.Find("Selection").GetComponent<Image>().enabled;
            _currSelected.Find("Selection").GetComponent<Image>().enabled = !isEnabled;
        }
    }
    private void Clear()
    {
        for (int i = 0; i < _buttons.childCount; i++)
        {
            var curr = _buttons.GetChild(i);
            curr.Find("Selection").GetComponent<Image>().enabled = false;
        }
    }
    private void IncSelection(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        Highlight();
        currentSelection += inc;
        if (currentSelection >= functions.Count)
            currentSelection = 0;
        if (currentSelection < 0)
            currentSelection = functions.Count - 1;
        _currSelected = _buttons.GetChild(currentSelection);
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
        Clear();
        _menuGrp.alpha = 0;
    }
}

public class ListLayer : MenuLayer {
    public static readonly int pageMax = 7;

    CanvasGroup _listFrame;
    Transform _listContent;
    GameObject _arrowLeft, _arrowRight;
    Image _closeSelect;
    TMP_Text _listPageIndicator;
    GameObject _listPrefab;
    Transform _currSelected;

    List<Transform> addedEntries;
    public int currPage;
    public int pageLimit;

    public ListLayer(CanvasGroup frame, TMP_Text page, Image close, Transform list, GameObject left, GameObject right, GameObject prefab)
    {
        addedEntries = new List<Transform>();
        _listFrame = frame;
        _listPageIndicator = page;
        _listContent = list;
        _arrowLeft = left;
        _arrowRight = right;
        _closeSelect = close;
        _listPrefab = prefab;
        _currSelected = null;
        currPage = 0;
    }

    public void ClearList()
    {
        addedEntries.Clear();
        for (int i = _listContent.childCount - 1; i >= 0; i--)
        {
            GameObject.Destroy(_listContent.GetChild(i).gameObject);
        }
        if (functions != null)
            functions.Clear();
    }

    public override void Open()
    {
        base.Open();
        if (addedEntries.Count > 0)
            _currSelected = addedEntries[currentSelection];
        _listFrame.alpha = 1;
        _listPageIndicator.text = "Page " + (currPage + 1) + " of " + pageLimit;
    }
    public override void Highlight()
    {
        if (_currSelected != null)
        {
            _currSelected.Find("BG").GetComponent<Image>().enabled = !_currSelected.Find("BG").GetComponent<Image>().enabled;
            _currSelected.Find("BGSelect").GetComponent<Image>().enabled = !_currSelected.Find("BGSelect").GetComponent<Image>().enabled;
        } else
        {
            _closeSelect.enabled = !_closeSelect.enabled;
        }
        _arrowLeft.SetActive(currPage != 0);
        _arrowRight.SetActive(currPage != pageLimit - 1 && pageLimit > 1);
    }

    public Transform AddEntry()
    {
        var newEntry = Transform.Instantiate(_listPrefab, _listContent).transform;
        addedEntries.Add(newEntry);
        return newEntry;
    }

    private void IncSelection(int inc, bool isHorizontal)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        if (isHorizontal)
        {
            if (currentSelection != functions.Count - 1)
            {
                Highlight();
                currPage = Mathf.Clamp(currPage + inc, 0, pageLimit - 1);
                refresh();
                if (currentSelection >= addedEntries.Count - 1)
                    currentSelection = addedEntries.Count - 1;
                _listPageIndicator.text = "Page " + (currPage + 1) + " of " + pageLimit;
                _currSelected = addedEntries[currentSelection];
                Highlight();
            }
        } else
        {
            Highlight();
            if (currentSelection != functions.Count - 1) {
                currentSelection += inc;
                if (currentSelection >= functions.Count - 1 || currentSelection < 0)
                {
                    currentSelection = functions.Count - 1;
                    _currSelected = null;
                } else
                {
                    _currSelected = addedEntries[currentSelection];
                }
            } else
            {
                currentSelection = Mathf.Clamp(inc > 0 ? 0 : addedEntries.Count - 1, 0, addedEntries.Count - 1);
                if (currentSelection < 0)
                    currentSelection = 0;
                if (addedEntries.Count > 0)
                    _currSelected = addedEntries[currentSelection];
            }
            Highlight();
        }
    }
    public override void Control(PlayerInput inputManager)
    {
        if (inputManager.actions["Up"].WasPressedThisFrame())
        {
            IncSelection(-1, false);
        }
        if (inputManager.actions["Down"].WasPressedThisFrame())
        {
            IncSelection(1, false);
        }
        if (inputManager.actions["Left"].WasPressedThisFrame())
        {
            IncSelection(-1, true);
        }
        if (inputManager.actions["Right"].WasPressedThisFrame())
        {
            IncSelection(1, true);
        }
    }
    public override void Close()
    {
        base.Close();
        _listFrame.alpha = 0;
        _closeSelect.enabled = false;
    }
    public override void OnRefresh()
    {
        base.OnRefresh();
        _listFrame.alpha = 1;
        _listPageIndicator.text = "Page " + (currPage + 1) + " of " + pageLimit;
    }
    public override void OnCreateFrameComplete()
    {
        base.OnCreateFrameComplete();
        if (currentSelection < addedEntries.Count)
            _currSelected = addedEntries[currentSelection];
        Highlight();
    }
}
public class DialogueLayer : MenuLayer
{
    CanvasGroup _dialogueFrame;
    Transform _options;
    Transform _currSelected;
    GameObject _listPrefab;
    List<Transform> addedEntries;
    public DialogueLayer(CanvasGroup frame, Transform opt, GameObject prefab)
    {
        _dialogueFrame = frame;
        _options = opt;
        _listPrefab = prefab;
        addedEntries = new List<Transform>();
        _currSelected = null;
    }
    public override void Open()
    {
        base.Open();
        _currSelected = addedEntries[currentSelection];
        Highlight();
        _dialogueFrame.alpha = 1;
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
        if (_currSelected != null)
        {
            _currSelected.Find("BG").GetComponent<Image>().enabled = !_currSelected.Find("BG").GetComponent<Image>().enabled;
            _currSelected.Find("BGSelect").GetComponent<Image>().enabled = !_currSelected.Find("BGSelect").GetComponent<Image>().enabled;
        }
    }
    private void IncSelection(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        Highlight();
        currentSelection += inc;
        if (currentSelection >= functions.Count)
            currentSelection = 0;
        if (currentSelection < 0)
            currentSelection = functions.Count - 1;
        _currSelected = addedEntries[currentSelection];
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
        _dialogueFrame.alpha = 0;
    }

    public override void OnRefresh()
    {
        base.OnRefresh();
        IncSelection(0);
        _dialogueFrame.alpha = 1;
    }
}
public class ReadLayer : MenuLayer
{
    CanvasGroup _readFrame;
    Image _closeSelect;
    public ReadLayer(CanvasGroup readFrame, Image closeSelect)
    {
        _readFrame = readFrame;
        _closeSelect = closeSelect;
    }
    public override void Open()
    {
        base.Open();
        Highlight();
        _readFrame.alpha = 1;
    }
    public override void Highlight()
    {
        _closeSelect.enabled = true;
    }
    public override void Control(PlayerInput inputManager)
    {

    }
    public override void Close()
    {
        base.Close();
        Highlight();
        _readFrame.alpha = 0;
    }
}

public class FreeRoamMenuHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Start")]
    [SerializeField] CanvasGroup _menuGrp;
    [SerializeField] Transform _menuButtons;
    [SerializeField] TMP_Text _goldText;

    [Header("List")]
    [SerializeField] CanvasGroup _listGrp;
    [SerializeField] TMP_Text _listTitle;
    [SerializeField] Transform _listContent;
    [SerializeField] Image _closeSelect;
    [SerializeField] GameObject _arrowLeft, _arrowRight;
    [SerializeField] TMP_Text _listPageIndicator;

    [Header("Dialogue")]
    [SerializeField] CanvasGroup _dialogueGrp;
    [SerializeField] Transform _dialogueContent;

    [Header("Prefab")]
    [SerializeField] GameObject _listEntry;

    [Header("Read")]
    [SerializeField] CanvasGroup _readGrp;
    [SerializeField] TMP_Text _readTitle;
    [SerializeField] TMP_Text _readContent;
    [SerializeField] Image _readCloseSelect;

    private PlayerInput _inputManager;
    private List<MenuLayer> _layers;

    public bool IsOpen
    {
        get
        {
            if (_layers == null) return false;
            return _layers.Count > 0;
        }
    }

    private MenuLayer CurrentLayer
    {
        get {
            if (_layers.Count < 1)
                return null;
            else
                return _layers.Last();
        }
    }

    private List<StaticMenuFunction> _othersFunctions;

    private void Start()
    {
        _inputManager = GetComponent<PlayerInput>();
        _layers = new List<MenuLayer>();
        _othersFunctions = new List<StaticMenuFunction>();
        _othersFunctions.Add(new StaticMenuFunction("Controls", delegate
        {
            CreateReadLayer("Controls", "Arrow Keys: Up, Down, Left, Right\n\nZ: Accept/Interact\nX: Decline/Open Menu\nLeft Shift: Anchor Position");
        }));
        _othersFunctions.Add(new StaticMenuFunction("Item Guide", delegate
        {
            CreateReadLayer("Item Guide", "Use items in dungeons. Some items will also apply special effects to your character if they hold them.");
        }));
        _othersFunctions.Add(new StaticMenuFunction("Quest Guide", delegate
        {
            CreateReadLayer("Quest Guide", "Head to the Guild Hall to take a quest. Activate it in the menu before going into the dungeon the quest gave you. Complete quests for Adventurer XP, Items and Gold.");
        }));
    }

    private void PerformFunction()
    {
        if (CurrentLayer != null)
        {
            CurrentLayer.functions[CurrentLayer.CurrentSelection]();
        }
    }

    private void CreateReadLayer(string title, string content)
    {
        var rLayer = new ReadLayer(_readGrp, _readCloseSelect);
        rLayer.functions = new List<MenuLayer.MenuFunction>();
        rLayer.refresh = delegate
        {
            _readTitle.text = title;
            _readContent.text = content;
        };
        rLayer.functions.Add(rLayer.Close);
        _layers.Add(rLayer);
        CurrentLayer.Open();
    }
    private void OpenInventory()
    {
        var inventoryLayer = new ListLayer(_listGrp, _listPageIndicator, _closeSelect, _listContent, _arrowLeft, _arrowRight, _listEntry);
        inventoryLayer.refresh = delegate
        {
            _listTitle.text = "Inventory";
            inventoryLayer.pageLimit = (int)Mathf.Ceil((float)GlobalGameManager.Instance.inventory.Count / ListLayer.pageMax);
            inventoryLayer.ClearList();
            inventoryLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = inventoryLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(inventoryLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0,GlobalGameManager.Instance.inventory.Count); i++)
            {
                var newEntry = inventoryLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = GlobalGameManager.Instance.inventory[i].module.itemName;
                inventoryLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listEntry);
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();
                        var holdEntry = dialogueLayer.AddEntry();
                        holdEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Hold";
                        dialogueLayer.functions.Add(delegate
                        {
                            var pLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listEntry);
                            pLayer.refresh = delegate
                            {
                                pLayer.ClearList();
                                pLayer.functions = new List<MenuLayer.MenuFunction>();
                                for (int j = 0; j < GlobalGameManager.Instance.party.Count; j++)
                                {
                                    var character = GlobalGameManager.Instance.party[j];
                                    var memberEntry = pLayer.AddEntry();
                                    memberEntry.Find("ItemText").GetComponent<TMP_Text>().text = character.Profile.characterName;
                                    pLayer.functions.Add(delegate
                                    {
                                        GlobalGameManager.Instance.HoldItem(inventoryLayer.CurrentSelection, pLayer.CurrentSelection);
                                        pLayer.Close();
                                        dialogueLayer.Close();
                                    });
                                }

                                var pEntry = pLayer.AddEntry();
                                pEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Close";
                                pLayer.functions.Add(pLayer.Close);
                            };
                            _layers.Add(pLayer);
                            CurrentLayer.Open();
                        });

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            CreateReadLayer("About: " + GlobalGameManager.Instance.inventory[inventoryLayer.currPage * ListLayer.pageMax + inventoryLayer.CurrentSelection].module.itemName,
                                GlobalGameManager.Instance.inventory[inventoryLayer.currPage * ListLayer.pageMax + inventoryLayer.CurrentSelection].module.itemDescription);
                        });

                        var trashEntry = dialogueLayer.AddEntry();
                        trashEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Trash";
                        dialogueLayer.functions.Add(delegate
                        {
                            GlobalGameManager.Instance.inventory.RemoveAt(inventoryLayer.currPage * ListLayer.pageMax + inventoryLayer.CurrentSelection);
                            dialogueLayer.Close();
                        });

                        var closeEntry = dialogueLayer.AddEntry();
                        closeEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Close";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            }
            inventoryLayer.functions.Add(inventoryLayer.Close);
        };
        _layers.Add(inventoryLayer);
        CurrentLayer.Open();
    }

    private void OpenParty()
    {
        var partyLayer = new ListLayer(_listGrp, _listPageIndicator, _closeSelect, _listContent, _arrowLeft, _arrowRight, _listEntry);
        partyLayer.refresh = delegate
        {
            _listTitle.text = "Party";
            partyLayer.pageLimit = (int)Mathf.Ceil((float)GlobalGameManager.Instance.party.Count / ListLayer.pageMax);
            partyLayer.ClearList();
            partyLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = partyLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(partyLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, GlobalGameManager.Instance.party.Count); i++)
            {
                var partyMember = GlobalGameManager.Instance.party[i];
                var newEntry = partyLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = partyMember.Profile.characterName;
                partyLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listEntry);
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();
                        dialogueLayer.functions = new List<MenuLayer.MenuFunction>();

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            CreateReadLayer("About: " + partyMember.Profile.characterName, partyMember.GetDescription());
                        });

                        var closeEntry = dialogueLayer.AddEntry();
                        closeEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Close";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };
                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            }
            partyLayer.functions.Add(partyLayer.Close);
        };
        _layers.Add(partyLayer);
        CurrentLayer.Open();
    }
    private void OpenQuests()
    {
        var questLayer = new ListLayer(_listGrp, _listPageIndicator, _closeSelect, _listContent, _arrowLeft, _arrowRight, _listEntry);
        questLayer.refresh = delegate
        {
            _listTitle.text = "Quests";
            questLayer.pageLimit = (int)Mathf.Ceil((float)GlobalGameManager.Instance.ownedQuests.Count / ListLayer.pageMax);
            questLayer.ClearList();
            questLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = questLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(questLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, GlobalGameManager.Instance.ownedQuests.Count); i++)
            {
                var quest = GlobalGameManager.Instance.ownedQuests[i];
                var newEntry = questLayer.AddEntry();
                string title = quest.QuestTitleText;
                if (title.Length > 25)
                {
                    title = title.Substring(0, 25) + "...";
                }
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = title;
                if (quest.isActive)
                {
                    newEntry.Find("ItemText").GetComponent<TMP_Text>().color = Color.green;
                }
                questLayer.functions.Add(delegate
                {
                    var dialogueLayer = new DialogueLayer(_dialogueGrp, _dialogueContent, _listEntry);
                    dialogueLayer.ClearList();
                    dialogueLayer.functions = new List<MenuLayer.MenuFunction>();
                    dialogueLayer.refresh = delegate
                    {
                        dialogueLayer.ClearList();

                        var activateEntry = dialogueLayer.AddEntry();
                        if (quest.isActive)
                        {
                            activateEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Deactivate";
                            dialogueLayer.functions.Add(delegate
                            {
                                quest.isActive = false;
                                dialogueLayer.Close();
                            });
                        }
                        else
                        {
                            activateEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Activate";
                            dialogueLayer.functions.Add(delegate
                            {
                                quest.isActive = true;
                                dialogueLayer.Close();
                            });
                        }

                        var infoEntry = dialogueLayer.AddEntry();
                        infoEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Info";
                        dialogueLayer.functions.Add(delegate
                        {
                            string desc = string.Format("Title: {0}\n\n{1}\n{2}\n{3}\n{4}\n{5}", quest.QuestTitleText, quest.QuestClientText, 
                                quest.QuestPlaceText, quest.QuestObjectiveText, quest.QuestDifficultyText, quest.QuestRewardText);
                            CreateReadLayer("About: " + quest.clientName + "'s Quest", desc);
                        });

                        var trashEntry = dialogueLayer.AddEntry();
                        trashEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Trash";
                        dialogueLayer.functions.Add(delegate
                        {
                            GlobalGameManager.Instance.ownedQuests.Remove(quest);
                            dialogueLayer.Close();
                        });

                        var cEntry = dialogueLayer.AddEntry();
                        cEntry.Find("ItemText").GetComponent<TMP_Text>().text = "Close";
                        dialogueLayer.functions.Add(dialogueLayer.Close);
                    };

                    _layers.Add(dialogueLayer);
                    CurrentLayer.Open();
                });
            }
            questLayer.functions.Add(questLayer.Close);
        };

        _layers.Add(questLayer);
        CurrentLayer.Open();
    }
    private void OpenOthers()
    {
        var othersLayer = new ListLayer(_listGrp, _listPageIndicator, _closeSelect, _listContent, _arrowLeft, _arrowRight, _listEntry);
        othersLayer.refresh = delegate
        {
            _listTitle.text = "Others";
            othersLayer.pageLimit = (int)Mathf.Ceil((float)_othersFunctions.Count / ListLayer.pageMax);
            othersLayer.ClearList();
            othersLayer.functions = new List<MenuLayer.MenuFunction>();
            for (int i = othersLayer.currPage * ListLayer.pageMax; i < Mathf.Clamp(othersLayer.currPage * ListLayer.pageMax + ListLayer.pageMax, 0, _othersFunctions.Count); i++)
            {
                StaticMenuFunction item = _othersFunctions[i];
                var newEntry = othersLayer.AddEntry();
                newEntry.Find("ItemText").GetComponent<TMP_Text>().text = item.title;
                othersLayer.functions.Add(item.function);
            }
            othersLayer.functions.Add(othersLayer.Close);
        };
        _layers.Add(othersLayer);
        CurrentLayer.Open();
    }

    private void CreateMain()
    {
        _goldText.text = GlobalGameManager.Instance.ownedGold.ToString();

        var newLayer = new StartLayer(_menuButtons, _menuGrp);
        newLayer.refresh = delegate
        {
            newLayer.functions = new List<MenuLayer.MenuFunction>();
            newLayer.functions.Add(OpenInventory);
            newLayer.functions.Add(OpenParty);
            newLayer.functions.Add(OpenQuests);
            newLayer.functions.Add(OpenOthers);
            newLayer.functions.Add(newLayer.Close);
        };
        _layers.Add(newLayer);
        CurrentLayer.Open();
    }

    private void Update()
    {
        if (CurrentLayer != null)
        {
            if (CurrentLayer.nextFrameTrigger)
            {
                CurrentLayer.OnCreateFrameComplete();
                CurrentLayer.nextFrameTrigger = false;
            }
            CurrentLayer.Control(_inputManager);
            if (_inputManager.actions["Accept"].WasPressedThisFrame())
            {
                PerformFunction();
            }
            if (_inputManager.actions["Decline"].WasPressedThisFrame())
            {
                CurrentLayer.Close();
            }
            if (!CurrentLayer.IsOpen)
            {
                CurrentLayer.Close();
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
        } else
        {
            if (_inputManager.actions["Decline"].WasPressedThisFrame())
            {
                FRController controller = FindAnyObjectByType<FRController>();
                if (controller != null)
                {
                    if (controller.CanControl)
                        CreateMain();
                }
            }
        }
    }
}
