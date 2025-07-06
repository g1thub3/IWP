using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class DungeonUIHandler : MonoBehaviour
{
    [Header("Canvas Groups")]
    public CanvasGroup displayGrp;
    public CanvasGroup combatGrp;
    public CanvasGroup menuGrp;
    public CanvasGroup endscreenGrp;
    public DungeonMenuHandler menu;
    public DungeonTransitionHandler transitioner;

    [Header("Displays")]
    public TMP_Text dungeonNameText;
    public TMP_Text floorText;

    [Header("Combat")]
    [SerializeField] private Transform _turnLogContent;
    [SerializeField] private GameObject _turnLogEntry;

    [SerializeField] private RectTransform _leaderHPBar, _leaderENBar, _leaderMNBar, _leaderHGBar;
    [SerializeField] private TMP_Text _leaderNameLabel, _leaderLvlLabel, _leaderHPLabel, _leaderENLabel, _leaderMNLabel, _leaderHGLabel;
    [SerializeField] private GameObject _partyMemberEntry;
    [SerializeField] private Transform _memberList;

    public struct PartyMemberUIEntry {
        public Image sprite;
        public RectTransform hpAmt; // height: 300
        public RectTransform hungerAmt; // height: 100
        public TMP_Text charName;
        public TMP_Text charLevel;
        public Image deathCover;
        public static PartyMemberUIEntry New(Transform obj)
        {
            PartyMemberUIEntry newData = new PartyMemberUIEntry();
            newData.sprite = obj.Find("Mask").Find("Sprite").GetComponent<Image>();
            newData.hpAmt = obj.Find("HPBar").Find("Amount").GetComponent<RectTransform>();
            newData.hungerAmt = obj.Find("HungerBar").Find("Amount").GetComponent<RectTransform>();
            newData.charName = obj.Find("CharacterName").GetComponent<TMP_Text>();
            newData.charLevel = obj.Find("CharacterLevel").GetComponent<TMP_Text>();
            newData.deathCover = obj.Find("DeathCover").GetComponent<Image>();
            return newData;
        }
    }

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

    private Dictionary<CharacterEntry, PartyMemberUIEntry> _partyUIDictionary;

    private void Start()
    {
        _gameManager = FindAnyObjectByType<DGGameManager>();
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _partyUIDictionary = new Dictionary<CharacterEntry, PartyMemberUIEntry>();
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
            if (q.competitiveLevel > 0)
            {
                label = string.Format("<i>{0}</i>", label);
            }
            textcomp.text = label;
            if (_gameManager.CurrentFloor == q.quest.floor)
            {
                textcomp.color = Color.yellow;
            }
            if (!q.quest.questPossible)
            {
                textcomp.color = Color.red;
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
            case DUNGEON_END_CONTEXT.QUEST_FAIL:
                _endMsg.text = "Quest Failed...";
                _blurb.text = "Another adventurer completed your quest before you did, so you left the dungeon safely.";
                break;
        }
        _dgExplored.text = "Dungeon Explored: " + data.dungeonName;
        _finalFloor.text = "Final Floor: " + (gameManager.CurrentFloor - 1);
        _remainingFloors.text = "Remaining Floors: " + (data.floorCount - (gameManager.CurrentFloor - 1));
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
                else
                    _leaderHPLabel.color = Color.white;
                break;
            case CHARACTER_STAT.ENERGY: //350
                _leaderENBar.offsetMax = new Vector2(_leaderENBar.offsetMax.x, 350 * (perc - 1));
                _leaderENLabel.text = curr.ToString();
                if (curr == max)
                    _leaderENLabel.color = _maxENCol;
                else
                    _leaderENLabel.color = Color.white;
                break;
            case CHARACTER_STAT.MANA: //350
                _leaderMNBar.offsetMax = new Vector2(_leaderMNBar.offsetMax.x, 350 * (perc - 1));
                _leaderMNLabel.text = curr.ToString();
                if (curr == max)
                    _leaderMNLabel.color = _maxMNCol;
                else
                    _leaderMNLabel.color = Color.white;
                break;
            case CHARACTER_STAT.HUNGER: //175
                _leaderHGBar.offsetMax = new Vector2(_leaderHGBar.offsetMax.x, 175 * (perc - 1));
                _leaderHGLabel.text = curr.ToString();
                if (curr == max)
                    _leaderHGLabel.color = _maxHGCol;
                else
                    _leaderHGLabel.color = Color.white;
                break;
        }
    }
    public void UpdateLeaderInfo()
    {
        _leaderNameLabel.text = GlobalGameManager.Instance.party[0].Profile.characterName;
        _leaderLvlLabel.text = "Lv. " + GlobalGameManager.Instance.party[0].characterLevel;
    }
    public void RegisterPlayer(DGPlayer newPlr)
    {
        _focusedPlr = newPlr;
        _inputManager = _focusedPlr.GetComponent<PlayerInput>();
        _focusedPlr.OnLeaderStatChanged += UpdateLeaderUI;
        _focusedPlr.OnLeaderLevelChanged += UpdateLeaderWhole;
        UpdateLeaderWhole();
    }

    public void RegisterParty(List<CharacterBehaviour> activeParty)
    {
        if (activeParty.Count < 2) return;
        for (int i = 1; i < activeParty.Count; i++) {
            var newEntry = Instantiate(_partyMemberEntry, _memberList);
            PartyMemberUIEntry newEntryData = PartyMemberUIEntry.New(newEntry.transform);
            CharacterEntry character = activeParty[i].character;
            _partyUIDictionary.Add(character, newEntryData);

            newEntryData.sprite.sprite = character.Profile.characterSprite;
            newEntryData.charName.text = character.Profile.characterName;
            newEntryData.charLevel.text = "Lv. " + character.characterLevel;
        }
    }

    public void UpdatePartyStatus(CharacterBehaviour changedMember)
    {
        if (!_partyUIDictionary.ContainsKey(changedMember.character)) return;
        var ui = _partyUIDictionary[changedMember.character];
        ui.charLevel.text = "Lv. " + changedMember.character.characterLevel;
        float perc = (float)changedMember.health / changedMember.character.maxHealth.CurrStat;
        ui.hpAmt.offsetMax = new Vector2(ui.hpAmt.offsetMax.x, 300 * (perc - 1));
        perc = (float)changedMember.hunger / changedMember.character.hungerSize.CurrStat;
        ui.hungerAmt.offsetMax = new Vector2(ui.hungerAmt.offsetMax.x, 100 * (perc - 1));
        if (changedMember.health <= 0)
            ui.deathCover.enabled = true;
    }

    private void UpdateLeaderWhole()
    {
        var cb = _focusedPlr.GetComponent<CharacterBehaviour>();
        UpdateLeaderInfo();
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
        foreach (var tile in _dungeonGen.CurrentFloor.nonWallTiles)
        {
            var pt = Instantiate(_minimapPoint, _minimapContainer);
            pt.GetComponent<RectTransform>().anchoredPosition = new Vector2(tile.coord.x * 10, tile.coord.z * 10);
            pt.name = _dungeonGen.CurrentFloor.CoordToIndex(tile.coord).ToString();
        }
        UpdateMinimap();
    }

    public void UpdateMinimap()
    {
        for (int i = 0; i < _minimapContainer.childCount; i++)
        {
            MinimapPoint pt = _minimapContainer.GetChild(i).GetComponent<MinimapPoint>();
            TileInfo tile = _dungeonGen.CurrentFloor.tiles[int.Parse(pt.name)];
            pt.image.enabled = tile.hasBeenDiscovered;
            pt.staircase.enabled = tile.structure != null && tile.hasBeenDiscovered;
            pt.item.enabled = tile.item != null && tile.hasBeenDiscovered;

            float dist = -1;
            if (_focusedPlr != null)
            {
                dist = tile.coord.DistanceSquared(_focusedPlr.Position);
            }

            pt.player.enabled = false;
            pt.enemy.enabled = false;
            pt.questrescue.enabled = false;
            pt.party.enabled = false;
            pt.competition.enabled = false;
            if (tile.occupyingEntity != null)
            {
                if (tile.occupyingEntity is DGPlayer)
                {
                    pt.player.enabled = true;
                }
                else if (tile.occupyingEntity.GetComponent<CharacterBehaviour>().alliance == 0)
                {
                    pt.party.enabled = true;
                }
                else if (tile.occupyingEntity.GetComponent<CharacterBehaviour>().alliance == -1)
                {
                    pt.questrescue.enabled = (dist <= GlobalGameManager.Instance.party[0].viewDistance && dist != -1) || (_focusedPlr.CurrentRoom == tile.occupyingEntity.CurrentRoom && _focusedPlr.CurrentRoom != null);
                }
                else if (tile.occupyingEntity.GetComponent<DGNPC>().associatedCompetitor != null)
                {
                    pt.competition.enabled = (dist <= GlobalGameManager.Instance.party[0].viewDistance && dist != -1) || (_focusedPlr.CurrentRoom == tile.occupyingEntity.CurrentRoom && _focusedPlr.CurrentRoom != null);
                }
                else
                {
                    pt.enemy.enabled = (dist <= GlobalGameManager.Instance.party[0].viewDistance && dist != -1) || (_focusedPlr.CurrentRoom == tile.occupyingEntity.CurrentRoom && _focusedPlr.CurrentRoom != null);
                }
            }
        }
    }
}
