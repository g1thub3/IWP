using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DungeonUIHandler : MonoBehaviour
{
    [Header("Canvas Groups")]
    public CanvasGroup displayGrp;
    public CanvasGroup combatGrp;
    public CanvasGroup menuGrp;
    public CanvasGroup endscreenGrp;
    public DungeonMenuHandler menu;

    [Header("Displays")]
    public TMP_Text dungeonNameText;
    public TMP_Text floorText;

    [Header("Combat")]
    [SerializeField] private Transform _turnLogContent;
    [SerializeField] private GameObject _turnLogEntry;

    [SerializeField] private RectTransform _leaderHPBar, _leaderENBar, _leaderMNBar, _leaderHGBar;
    [SerializeField] private TMP_Text _leaderNameLabel, _leaderLvlLabel, _leaderHPLabel, _leaderENLabel, _leaderMNLabel, _leaderHGLabel;

    [Header("Endscreen")]
    [SerializeField] private TMP_Text _endMsg;
    [SerializeField] private TMP_Text _dgExplored;
    [SerializeField] private TMP_Text _finalFloor;
    [SerializeField] private TMP_Text _remainingFloors;
    [SerializeField] private TMP_Text _heldItem;
    [SerializeField] private TMP_Text _lvlExp;
    [SerializeField] private TMP_Text _pa;
    [SerializeField] private TMP_Text _pd;
    [SerializeField] private TMP_Text _ma;
    [SerializeField] private TMP_Text _md;
    [SerializeField] private TMP_Text _blurb;

    [Header("Values")]
    [SerializeField] private Color _maxHPCol;
    [SerializeField] private Color _maxENCol;
    [SerializeField] private Color _maxMNCol;
    [SerializeField] private Color _maxHGCol;

    [Header("Quest Display")]
    [SerializeField] private Transform _questContent;
    [SerializeField] private GameObject _listEntry;

    [Header("Minimap")]
    [SerializeField] private Transform _minimapContainer;
    [SerializeField] private GameObject _minimapPoint;

    private DGPlayer _focusedPlr;
    private PlayerInput _inputManager;
    private DGGameManager _gameManager;
    private DGGenerator _dungeonGen;
    private void Start()
    {
        _gameManager = FindAnyObjectByType<DGGameManager>();
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
    }

    public void UpdateQuestUI()
    {
        for (int i = _questContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_questContent.GetChild(i).gameObject);
        }
        for (int i = 0; i < _gameManager.ActiveQuests.Count;i++)
        {
            var q = _gameManager.ActiveQuests[i];
            var entry = Instantiate(_listEntry, _questContent);
            TMP_Text textcomp = entry.GetComponentInChildren<TMP_Text>();
            string label = "(" + (!GlobalGameManager.Instance.selectedDungeon.isAscending ? "B" : string.Empty) + q.quest.floor + "F) " + q.QuestObjectiveText;
            textcomp.text = label;
            if (_gameManager.CurrentFloor == q.quest.floor)
            {
                textcomp.color = Color.yellow;
            }
            if (q.quest.questCompleted)
            {
                textcomp.color = Color.green;
            }
        }
    }

    public void ClearTurnLog()
    {
        for (int i = _turnLogContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_turnLogContent.GetChild(i).gameObject);
        }
    }
    public void AddEntry(string message)
    {
        var msg = Instantiate(_turnLogEntry);
        msg.GetComponent<TMP_Text>().text = message;
        msg.transform.SetParent(_turnLogContent, false);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_turnLogContent.GetComponent<RectTransform>());
    }
    public void Endscreen(DUNGEON_END_CONTEXT context, DGGameManager gameManager, DGData data)
    {
        displayGrp.alpha = combatGrp.alpha = menuGrp.alpha = 0;
        endscreenGrp.alpha = 1;
        switch (context)
        {
            case DUNGEON_END_CONTEXT.COMPLETED:
                _endMsg.text = "Dungeon Completed!";
                _blurb.text = "You went through all the floors and reached the end!";
                break;
            case DUNGEON_END_CONTEXT.LOSS:
                _endMsg.text = "Defeated...";
                _blurb.text = "You were defeated while in the dungeon...";
                break;
            case DUNGEON_END_CONTEXT.ESCAPE:
                _endMsg.text = "Escaped...";
                _blurb.text = "You escaped the dungeon and didn't complete it...";
                break;
            case DUNGEON_END_CONTEXT.QUEST:
                _endMsg.text = "Quest Completed!";
                _blurb.text = "You left the dungeon after completing a quest!";
                break;
        }
        _dgExplored.text = "Dungeon Explored: " + data.dungeonName;
        _finalFloor.text = "Final Floor: " + gameManager.CurrentFloor;
        _remainingFloors.text = "Remaining Floors: " + (data.floorCount - gameManager.CurrentFloor);
        var leader = GlobalGameManager.Instance.party[0];
        if (leader.HeldItem != null && leader.HeldItem.module != null)
        {
            _heldItem.text = leader.HeldItem.module.itemName;
        } else
        {
            _heldItem.text = "Held Item: None";
        }
        _lvlExp.text = "Level: " + leader.characterLevel + " | Exp: " + leader.experiencePoints;
        _pa.text = "PA: " + leader.physAtk.CurrStat;
        _pd.text = "PD: " + leader.physDef.CurrStat;
        _ma.text = "MA: " + leader.magicAtk.CurrStat;
        _md.text = "MD: " + leader.magicDef.CurrStat;
    }
    private void UpdateLeaderUI(CHARACTER_STAT statToUpdate, int curr, int max)
    {
        float perc = (float)curr / (float)max;

        switch(statToUpdate)
        {
            case CHARACTER_STAT.HEALTH: //400
                _leaderHPBar.offsetMax = new Vector2(_leaderHPBar.offsetMax.x, 400 * (perc - 1));
                _leaderHPLabel.text = curr.ToString();
                if (curr == max)
                    _leaderHPLabel.color = _maxHPCol;
                break;
            case CHARACTER_STAT.ENERGY: //350
                _leaderENBar.offsetMax = new Vector2(_leaderENBar.offsetMax.x, 350 * (perc - 1));
                _leaderENLabel.text = curr.ToString();
                if (curr == max)
                    _leaderENLabel.color = _maxENCol;
                break;
            case CHARACTER_STAT.MANA: //350
                _leaderMNBar.offsetMax = new Vector2(_leaderMNBar.offsetMax.x, 350 * (perc - 1));
                _leaderMNLabel.text = curr.ToString();
                if (curr == max)
                    _leaderMNLabel.color = _maxMNCol;
                break;
            case CHARACTER_STAT.HUNGER: //175
                _leaderHGBar.offsetMax = new Vector2(_leaderHGBar.offsetMax.x, 175 * (perc - 1));
                _leaderHGLabel.text = curr.ToString();
                if (curr == max)
                    _leaderHGLabel.color = _maxHGCol;
                break;
        }
    }
    public void RegisterPlayer(DGPlayer newPlr)
    {
        _focusedPlr = newPlr;
        _inputManager = _focusedPlr.GetComponent<PlayerInput>();
        _focusedPlr.OnLeaderStatChanged += UpdateLeaderUI;

        var cb = newPlr.GetComponent<CharacterBehaviour>();

        _leaderNameLabel.text = cb.character.Profile.characterName;
        _leaderLvlLabel.text = "Lv. " + cb.character.characterLevel.ToString();

        UpdateLeaderUI(CHARACTER_STAT.HEALTH, cb.health, cb.character.maxHealth.CurrStat);
        UpdateLeaderUI(CHARACTER_STAT.ENERGY, cb.energy, cb.character.maxEnergy.CurrStat);
        UpdateLeaderUI(CHARACTER_STAT.MANA, cb.mana, cb.character.maxMana.CurrStat);
        UpdateLeaderUI(CHARACTER_STAT.HUNGER, cb.hunger, cb.character.hungerSize.CurrStat);
    }
    public void ToggleMenu()
    {
        if (menu.IsOpen)
        {
            combatGrp.alpha = 1;
            menuGrp.alpha = 0;
        } else
        {
            menu.LoadParty();
            combatGrp.alpha = 0;
            menuGrp.alpha = 1;
        }
    }

    public void LoadMinimap()
    {
        for (int i = _minimapContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_minimapContainer.GetChild(i).gameObject);
        }
        foreach (var tile in _dungeonGen.CurrentFloor.tiles)
        {
            if (!tile.isWall)
            {
                var pt = Instantiate(_minimapPoint, _minimapContainer);
                pt.GetComponent<RectTransform>().anchoredPosition = new Vector2(tile.coord.x * 10, tile.coord.z * 10);
                pt.name = _dungeonGen.CurrentFloor.CoordToIndex(tile.coord).ToString();
            }
        }
        UpdateMinimap();
    }

    public void UpdateMinimap()
    {
        for (int i = 0; i < _minimapContainer.childCount; i++)
        {
            GameObject pt = _minimapContainer.GetChild(i).gameObject;
            TileInfo tile = _dungeonGen.CurrentFloor.tiles[int.Parse(pt.name)];
            pt.GetComponent<Image>().enabled = tile.hasBeenDiscovered;
            pt.transform.Find("Staircase").GetComponent<Image>().enabled = tile.structure != null && tile.hasBeenDiscovered;
            pt.transform.Find("Item").GetComponent<Image>().enabled = tile.item != null && tile.hasBeenDiscovered;

            if (tile.occupyingEntity != null)
            {
                if (tile.occupyingEntity is DGPlayer)
                {
                    pt.transform.Find("Player").GetComponent<Image>().enabled = true;
                    pt.transform.Find("Enemy").GetComponent<Image>().enabled = false;
                }
                else
                {
                    pt.transform.Find("Player").GetComponent<Image>().enabled = false;
                    DGPlayer plr = FindAnyObjectByType<DGPlayer>();
                    if (plr != null)
                    {
                        float dist = tile.coord.DistanceSquared(plr.Position);
                        pt.transform.Find("Enemy").GetComponent<Image>().enabled = dist <= 5  || (plr.CurrentRoom == tile.occupyingEntity.CurrentRoom && plr.CurrentRoom != null);
                    }
                }
            } else
            {
                pt.transform.Find("Player").GetComponent<Image>().enabled = false;
                pt.transform.Find("Enemy").GetComponent<Image>().enabled = false;
            }
        }
    }
}
