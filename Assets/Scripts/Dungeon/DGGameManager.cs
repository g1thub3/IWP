using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum DUNGEON_END_CONTEXT
{
    COMPLETED,
    LOSS,
    ESCAPE,
    QUEST,
    QUEST_FAIL
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
    private Dictionary<Quest, List<QuestCompetitor>> _questCompetition;

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
        _questCompetition.Clear();
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

    private IEnumerator WaitForInputQuestComplete(bool hasCompleted = true, Quest questFailed = null)
    {
        while (GlobalCanvasManager.Instance.PromptHandler.IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        if (GlobalCanvasManager.Instance.PromptHandler.TakeAnswer() == 0)
        {
            //progress floor
            QuestComplete(hasCompleted);
        } else
        {
            if (questFailed != null)
            {
                EndCompetition(questFailed);
            }
        }
    }

    public void QuestCompletePrompt()
    {
        PromptInfo prompt = new PromptInfo();
        prompt.message = "You completed a quest! Would you like to leave the dungeon now?";
        prompt.options = new string[2];
        prompt.options[0] = "Yes";
        prompt.options[1] = "No";

        CanvasGroup[] hidden = new CanvasGroup[1];
        hidden[0] = _dungeonUI.combatGrp;

        GlobalCanvasManager.Instance.PromptHandler.Prompt(prompt, hidden);
        StartCoroutine(WaitForInputQuestComplete());
    }

    public void QuestFailPrompt(Quest questFailed)
    {
        PromptInfo prompt = new PromptInfo();
        prompt.message = "Another adventurer completed one of your quests before you could. Would you like to leave the dungeon now?";
        prompt.options = new string[2];
        prompt.options[0] = "Yes";
        prompt.options[1] = "No";

        CanvasGroup[] hidden = new CanvasGroup[1];
        hidden[0] = _dungeonUI.combatGrp;

        GlobalCanvasManager.Instance.PromptHandler.Prompt(prompt, hidden);
        StartCoroutine(WaitForInputQuestComplete(false, questFailed));
    }

    public void QuestComplete(bool hasCompleted = true)
    {
        _isGameActive = false;
        _dungeonUI.Endscreen(hasCompleted ? DUNGEON_END_CONTEXT.QUEST : DUNGEON_END_CONTEXT.QUEST_FAIL, this, GlobalGameManager.Instance.selectedDungeon);
        isPressingInit = _inputManager.actions["Accept"].IsPressed();
        StartCoroutine(WaitForInput());
    }

    private void PlayerLoss(bool onDeath)
    {
        _isGameActive = false;
        _dungeonUI.Endscreen(onDeath ? DUNGEON_END_CONTEXT.LOSS : DUNGEON_END_CONTEXT.ESCAPE, this, GlobalGameManager.Instance.selectedDungeon);
        LoseItems();
        isPressingInit = _inputManager.actions["Accept"].IsPressed();
        StartCoroutine(WaitForInput());
    }
    
    private List<QuestCompetitor> GenerateQuestCompetitors(Quest quest)
    {
        var newList = new List<QuestCompetitor>();
        int amt = (int)Mathf.Ceil(quest.competitiveLevel * ((float)quest.competitiveLevel * 0.5f)) + 1;
        int possibleCompetitors = Random.Range(quest.competitiveLevel, amt);
        for (int i = 0; i < possibleCompetitors; i++)
        {
            var newCompetitor = new QuestCompetitor();
            newCompetitor.associatedQuest = quest;
            newCompetitor.party = new List<CharacterEntry>();
            newCompetitor.competitorName = CharacterProfiles.Instance.questNPCNames[Random.Range(0, CharacterProfiles.Instance.questNPCNames.Count)];
            newCompetitor.currentFloor = Random.Range(1, Mathf.Min(quest.quest.floor - 1, 5));
            newCompetitor.defaultProgress = Random.Range(QuestCompetitor.minProgress, QuestCompetitor.maxProgress);
            newCompetitor.floorProgress = newCompetitor.defaultProgress;

            int partySize = Random.Range(1,5);
            for (int j = 0; j < partySize; j++)
            {
                var newCharacter = CharacterEntry.Create(
                    CharacterProfiles.Instance.possibleCompetitors[Random.Range(0, CharacterProfiles.Instance.possibleCompetitors.Count)],
                    GlobalGameManager.Instance.party[0].characterLevel);
                if (j == 0)
                {
                    newCharacter.characterName = "Adventurer " + newCompetitor.competitorName;
                } else
                {
                    newCharacter.characterName = "Adventurer " + newCompetitor.competitorName + "'s Party Member " + j;
                }
                newCompetitor.party.Add(newCharacter);
            }

            newList.Add(newCompetitor);
        }
        return newList;
    }

    private void GetActiveQuests()
    {
        _activeQuests.Clear();
        for (int i = 0; i < GlobalGameManager.Instance.ownedQuests.Count; i++)
        {
            var questData = GlobalGameManager.Instance.ownedQuests[i];
            if (questData.quest.dungeon == GlobalGameManager.Instance.selectedDungeon && questData.isActive)
            {

                _activeQuests.Add(questData);
                if (questData.competitiveLevel > 0)
                {
                    _questCompetition.Add(questData, GenerateQuestCompetitors(questData));
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
    }
    private IEnumerator ImplementQuest()
    {
        _dungeonUI.LoadMinimap();
        yield return new WaitForEndOfFrame();

        for (int i = ActiveQuests.Count - 1; i >= 0; i--)
        {
            var questData = GlobalGameManager.Instance.ownedQuests[i];
            if (_questCompetition.ContainsKey(questData))
            {
                foreach (var competitor in _questCompetition[questData])
                {
                    competitor.target = _dungeonGen.CurrentFloor.stairs;
                    if (competitor.currentFloor != CurrentFloor)
                    {
                        competitor.floorProgress = competitor.defaultProgress;
                    } else
                    {
                        _dungeonGen.SpawnCompetitors(competitor);
                    }
                }
            }
            if (questData.quest.floor == CurrentFloor && questData.quest.questPossible) // Add target
            {
                var success = questData.quest.Execute(questData, _dungeonGen);
                if (success == null)
                {
                    questData.isActive = false;
                    if (_questCompetition.ContainsKey(questData))
                    {
                        _questCompetition.Remove(questData);
                    }
                    _activeQuests.RemoveAt(i);
                    Debug.Log("Failed to spawn quest item.");
                    continue;
                }
                if (_questCompetition.ContainsKey(questData))
                {
                    foreach (var competitor in _questCompetition[questData])
                    {
                        competitor.target = success;
                    }
                }
            }
        }

        if (_questCompetition.Count > 0)
        {
            _dungeonUI.transitioner.LoadCompetition(_questCompetition);
            _dungeonUI.transitioner.ToggleQuestComp(true);
        }

        _dungeonUI.UpdateQuestUI();
        RefreshTurnList();

        StartCoroutine(_dungeonUI.transitioner.FadeTransition(false, 0.75f, 3.0f, _dungeonUI.transitioner.floorDispGrp, delegate
        {
            StartCoroutine(_dungeonUI.transitioner.FadeTransition(false, 0.75f, 0.0f, _dungeonUI.transitioner.grp, delegate
            {
                _isGameActive = true;
            }));
        }));
    }

    public void RefreshTurnList()
    {
        turnList.Clear();
        List<DGEntity> entities = _dungeonGen.ActiveEntities;
        foreach (DGEntity entity in entities)
        {
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
    }

    public void RefreshGame()
    {
        if (_currentFloor > GlobalGameManager.Instance.selectedDungeon.floorCount)
        {
            _dungeonUI.Endscreen(DUNGEON_END_CONTEXT.COMPLETED, this, GlobalGameManager.Instance.selectedDungeon);
            isPressingInit = _inputManager.actions["Accept"].IsPressed();
            StartCoroutine(WaitForInput());
            return;
        }
        _dungeonUI.transitioner.ToggleQuestComp(false);
        StartCoroutine(_dungeonUI.transitioner.FadeTransition(true, 1, 0.25f, _dungeonUI.transitioner.floorDispGrp, delegate
        {
            _dungeonGen.NewFloor();
            if (GlobalGameManager.Instance.selectedDungeon.isAscending)
            {
                _dungeonUI.floorText.text = "Floor\n" + _currentFloor + "F";
            }
            else
            {
                _dungeonUI.floorText.text = "Floor\nB" + _currentFloor + "F";
            }

            StartCoroutine(ImplementQuest());
        }));
    }

    public void ToNextFloor()
    {
        _isGameActive = false;
        StartCoroutine(_dungeonUI.transitioner.FadeTransition(true, 1, 0.0f, _dungeonUI.transitioner.grp, delegate
        {
            _currentFloor++;
            if (GlobalGameManager.Instance.selectedDungeon.isAscending)
            {
                _dungeonUI.transitioner.SetFloorText(CurrentFloor + "F");
            }
            else
            {
                _dungeonUI.transitioner.SetFloorText("B" + CurrentFloor + "F");
            }
            RefreshGame();
        }));
    }

    private void LoseItems()
    {
        foreach (var quest in GlobalGameManager.Instance.ownedQuests)
        {
            quest.quest.questCompleted = false;
            if (quest.competitiveLevel > 0)
                quest.quest.questPossible = false;
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
        var prev = CurrentEntityTurn();
        if (prev != null)
        {
            prev.GetComponent<CharacterBehaviour>().OnTurnComplete();
            if (prev is DGPlayer) // PROGRESS THE COMPETITORS
            {
                foreach (var q in ActiveQuests)
                {
                    if (_questCompetition.ContainsKey(q))
                    {
                        foreach (var comp in _questCompetition[q])
                        {
                            if (comp.currentFloor == CurrentFloor) continue;
                            int floor = comp.currentFloor;
                            comp.Progress();
                            if (comp.currentFloor == floor)
                            {
                                switch (comp.floorProgress)
                                {
                                    case 15:
                                        if (comp.currentFloor != q.quest.floor)
                                            _dungeonUI.AddEntry(comp.competitorName + " is approaching the next floor!");
                                        else
                                            _dungeonUI.AddEntry(comp.competitorName + " is approaching the quest target!");
                                        break;
                                    case 30:
                                        if (comp.currentFloor != q.quest.floor)
                                            _dungeonUI.AddEntry(comp.competitorName + " is close to finding the way to the next floor!");
                                        else
                                            _dungeonUI.AddEntry(comp.competitorName + " is close to finding the quest target!");
                                        break;
                                    case 60:
                                        if (comp.currentFloor != q.quest.floor)
                                            _dungeonUI.AddEntry(comp.competitorName + " has explored half of their current floor!");
                                        else
                                            _dungeonUI.AddEntry(comp.competitorName + " is halfway through their search for the quest target!");
                                        break;
                                    case 90:
                                        if (comp.currentFloor != q.quest.floor)
                                            _dungeonUI.AddEntry(comp.competitorName + " has begun exploring their current floor!");
                                        else
                                            _dungeonUI.AddEntry(comp.competitorName + " has begun searching for the quest target!");
                                        break;
                                }
                            }
                            else
                            {
                                _dungeonUI.AddEntry(comp.competitorName + " has entered a new floor!");
                                if (comp.currentFloor == q.quest.floor)
                                {
                                    _dungeonUI.AddEntry(comp.competitorName + " has reached the floor with the quest target!");
                                }
                            }
                            if (comp.currentFloor == CurrentFloor) // When competition arrives on this floor
                            {
                                _dungeonGen.SpawnCompetitors(comp);
                                RefreshTurnList();
                            }
                            else if (comp.currentFloor > q.quest.floor && q.quest.questPossible)
                            {
                                q.quest.questPossible = false;
                                _dungeonUI.UpdateQuestUI();
                                QuestFailPrompt(q);
                            }
                        }
                    }
                }
            }
        }
        currentTurn++;
        if (currentTurn >= turnList.Count)
            currentTurn = 0;
        //Debug.Log("Turn changed!");
        _dungeonUI.UpdateMinimap();
    }

    public void OnItemDropped(GameObject droppedItemObj)
    {
        var container = droppedItemObj.GetComponent<DGItemContainer>();
        if (!container.Item.IsQuestTarget) return;
        foreach (var q in ActiveQuests)
        {
            if (!(q.quest is RetrievalQuest)) continue;
            var rq = q.quest as RetrievalQuest;
            if (_questCompetition.ContainsKey(q))
            {
                if (container.Item == rq.ToRetrieve)
                {
                    var obj = droppedItemObj.GetComponent<DGObject>();
                    foreach (var comp in _questCompetition[q])
                    {
                        comp.target = obj;
                    }
                    break;
                }
            }
        }
    }
    public void OnCharacterDeath(CharacterBehaviour dead)
    {
        _dungeonUI.AddEntry(dead.gameObject.name + " has been defeated!");
        dead.DropItem();
        bool deadPartyFound = false;
        foreach (var q in ActiveQuests)
        {
            if (_questCompetition.ContainsKey(q))
            {
                var compList = _questCompetition[q];
                for (int j = compList.Count - 1; j >= 0; j--) { 
                    var comp = compList[j];
                    for (int i = comp.party.Count - 1; i >= 0; i--)
                    {
                        if (comp.party[i] == dead.character)
                        {
                            comp.party.RemoveAt(i);
                            deadPartyFound = true;
                            break;
                        }
                    }
                    if (comp.party.Count == 0)
                    {
                        _dungeonUI.AddEntry(comp.competitorName + "'s party is no longer exploring the dungeon!");
                        compList.RemoveAt(j);
                    }
                }
            }
            if (deadPartyFound) break;
        }
    }
    public void EndCompetition(Quest quest)
    {
        foreach (var competitior in _questCompetition[quest])
        {
            if (competitior.partySpawned != null && competitior.partySpawned.Count > 0)
            {
                for (int i = competitior.partySpawned.Count - 1; i >= 0; i--)
                {
                    RegisterRemoval(competitior.partySpawned[i]);
                }
            }
        }
        _questCompetition.Remove(quest);
    }

    public void RegisterRemoval(DGEntity dead)
    {
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
        _inputManager = GlobalCanvasManager.Instance.GlobalInput;
        OnEscape += delegate { PlayerLoss(false); };
        TurnCompleted += NextTurn;
        _questCompetition = new Dictionary<Quest, List<QuestCompetitor>>();
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _currentFloor = 0;
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _activeQuests = new List<Quest>();
        _dungeonUI.dungeonNameText.text = GlobalGameManager.Instance.selectedDungeon.dungeonName;
        turnList = new List<DGEntity>();
        _dungeonUI.transitioner.SetDungeonText(GlobalGameManager.Instance.selectedDungeon.dungeonName);

        GetActiveQuests();
        ToNextFloor();
    }
}
