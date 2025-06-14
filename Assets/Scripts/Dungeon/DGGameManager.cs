using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DUNGEON_END_CONTEXT
{
    COMPLETED,
    LOSS,
    ESCAPE,
    QUEST
}

public class DGGameManager : MonoBehaviour
{
    public System.Action OnEscape;
    private DGGenerator _dungeonGen;
    private DungeonUIHandler _dungeonUI;
    public List<DGEntity> turnList;
    public int currentTurn;
    public System.Action TurnCompleted;
    private bool _isGameActive;
    private int _currentFloor;

    private List<Quest> _activeQuests;
    public List<Quest> ActiveQuests
    {
        get { return _activeQuests; }
    }

    private PlayerInput _inputManager;
    private bool isPressingInit;

    public bool IsGameActive
    {
        get { return _isGameActive; }
    }
    public int CurrentFloor
    {
        get { return _currentFloor; }
    }

    public DGEntity CurrentEntityTurn()
    {
        if (turnList == null || currentTurn >= turnList.Count || !IsGameActive)
            return null;
        return turnList[currentTurn];
    }

    private IEnumerator WaitForInput()
    {
        bool cont = false;
        while (!cont)
        {
            if (!isPressingInit)
            {
                if (_inputManager.actions["Accept"].IsPressed())
                    cont = true;
            } else
            {
                isPressingInit = _inputManager.actions["Accept"].IsPressed();
            }
            yield return new WaitForEndOfFrame();
        }
        GlobalGameManager.Instance.DayOver = true;
        GameSceneManager.Instance.ToDorm();
    }

    public void QuestComplete()
    {
        _isGameActive = false;
        _dungeonUI.Endscreen(DUNGEON_END_CONTEXT.QUEST, this, GlobalGameManager.Instance.selectedDungeon);
        if (_inputManager == null)
        {
            _inputManager = FindAnyObjectByType<PlayerInput>();
        }
        isPressingInit = _inputManager.actions["Accept"].IsPressed();
        StartCoroutine(WaitForInput());
    }

    private void PlayerLoss(bool onDeath)
    {
        _isGameActive = false;
        _dungeonUI.Endscreen(onDeath ? DUNGEON_END_CONTEXT.LOSS : DUNGEON_END_CONTEXT.ESCAPE, this, GlobalGameManager.Instance.selectedDungeon);
        LoseItems();
        if (_inputManager == null)
        {
            _inputManager = FindAnyObjectByType<PlayerInput>();
        }
        isPressingInit = _inputManager.actions["Accept"].IsPressed();
        StartCoroutine(WaitForInput());
    }
    
    private IEnumerator ImplementQuest()
    {
        _activeQuests.Clear();
        yield return new WaitForEndOfFrame();
        // GENERATE RETRIEVAL QUEST
        for (int i = 0; i < GlobalGameManager.Instance.ownedQuests.Count; i++)
        {
            var questData = GlobalGameManager.Instance.ownedQuests[i];
            if (questData.quest.dungeon == GlobalGameManager.Instance.selectedDungeon && questData.isActive)
            {
                _activeQuests.Add(questData);
                if (questData.quest.floor == CurrentFloor) // Add target
                {
                    if (questData.quest is RetrievalQuest)
                    {
                        var room = _dungeonGen.GetRandomRoom();
                        var spawnTile = _dungeonGen.SearchRandomTileInRoom(room, SearchConditions.New(false));
                        if (spawnTile != null)
                        {
                            var rQuest = questData.quest as RetrievalQuest;
                            var questItem = Tilesets.Instance.ConstructItemInteractable(rQuest.ToRetrieve);
                            questItem.GetComponent<DGItemContainer>().Item.IsQuestTarget = true;
                            _dungeonGen.InsertItem(questItem, spawnTile);
                            Debug.Log(spawnTile.coord);
                        }
                        else
                        {
                            questData.isActive = false;
                            _activeQuests.Remove(_activeQuests.Last());
                            Debug.Log("Failed to spawn quest item.");
                        }
                    }
                }
            }
        }
        for (int i = 0; i < _activeQuests.Count; i++)
        {
            for (int j = i; j < _activeQuests.Count; j++)
            {
                if (_activeQuests[j].quest.floor < _activeQuests[i].quest.floor)
                {
                    var temp = _activeQuests[j];
                    _activeQuests[j] = _activeQuests[i];
                    _activeQuests[i] = temp;
                }
            }
        }
        _dungeonUI.UpdateQuestUI();
        _dungeonUI.LoadMinimap();
    }

    public void RefreshGame()
    {
        if (_currentFloor + 1 > GlobalGameManager.Instance.selectedDungeon.floorCount)
        {
            _isGameActive = false;
            _dungeonUI.Endscreen(DUNGEON_END_CONTEXT.COMPLETED, this, GlobalGameManager.Instance.selectedDungeon);
            if (_inputManager == null)
            {
                _inputManager = FindAnyObjectByType<PlayerInput>();
            }
            isPressingInit = _inputManager.actions["Accept"].IsPressed();
            StartCoroutine(WaitForInput());
            return;
        }
        _currentFloor++;
        _dungeonGen.NewFloor();
        turnList = new List<DGEntity>();
        List<DGEntity> entities = _dungeonGen.ActiveEntities;
        foreach (DGEntity entity in entities) { 
            if (entity.GetComponent<DGPlayer>())
            {
                turnList.Add(entity);
                break;
            }
        }
        foreach (DGEntity entity in entities)
        {
            if (entity.GetComponent<DGPlayer>() == null)
            {
                turnList.Add(entity);
            }
        }
        currentTurn = 0;
        _isGameActive = true;
        
        if (GlobalGameManager.Instance.selectedDungeon.isAscending)
        {
            _dungeonUI.floorText.text = "Floor\n" + _currentFloor + "F";
        } else
        {
            _dungeonUI.floorText.text = "Floor\nB" + _currentFloor + "F";
        }

        StartCoroutine(ImplementQuest());
    }

    private void LoseItems()
    {
        foreach (var quest in GlobalGameManager.Instance.ownedQuests)
        {
            quest.quest.questCompleted = false;
        }
        GlobalGameManager.Instance.ownedGold /= 2;
        for (int i = 0; i < GlobalGameManager.Instance.party.Count; i++)
        {
            GlobalGameManager.Instance.party[i].HeldItem = null;
        }
        for (int item = 0; item < GlobalGameManager.Instance.inventory.Count / 2; item++)
        {
            GlobalGameManager.Instance.inventory.RemoveAt(Random.Range(0, GlobalGameManager.Instance.inventory.Count));
        }
    }

    public void NextTurn()
    {
        currentTurn++;
        if (currentTurn >= turnList.Count)
            currentTurn = 0;
        //Debug.Log("Turn changed!");
        _dungeonUI.UpdateMinimap();
    }

    public void RegisterDeath(DGEntity dead)
    {
        _dungeonUI.AddEntry(dead.gameObject.name + " has been defeated!");
        dead.GetComponent<CharacterBehaviour>().DropItem();

        int deadTurnNo;
        for (deadTurnNo = 0; deadTurnNo < turnList.Count; deadTurnNo++)
        {
            if (turnList[deadTurnNo] == dead)
                break;
        }
        _dungeonGen.ActiveEntities.Remove(dead);
        if (_dungeonGen.ActiveParty.Contains(dead.GetComponent<CharacterBehaviour>()))
            _dungeonGen.ActiveParty.Remove(dead.GetComponent<CharacterBehaviour>());
        turnList.RemoveAt(deadTurnNo);
        if (deadTurnNo >= turnList.Count)
            currentTurn = 0;

        if (dead is DGPlayer)
        {
            PlayerLoss(true);
        }

        Destroy(dead.gameObject);
    }

    private void Start()
    {
        OnEscape += delegate { PlayerLoss(false); };
        TurnCompleted += NextTurn;
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _currentFloor = 0;
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _activeQuests = new List<Quest>();
        _dungeonUI.dungeonNameText.text = GlobalGameManager.Instance.selectedDungeon.dungeonName;
        RefreshGame();
    }
}
