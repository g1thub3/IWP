using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.EventSystems.EventTrigger;

public struct SearchConditions {
    public static SearchConditions New(bool hasItem = false, bool isWall = false, bool hasEntity = false)
    {
        SearchConditions conditions = new SearchConditions();
        conditions.HasItem = hasItem;
        conditions.IsWall = isWall;
        conditions.HasEntity = hasEntity;
        return conditions;
    }
    public bool AreConditionsMet(TileInfo tile)
    {
        return ((tile.item != null) == HasItem && tile.isWall == IsWall) && (tile.occupyingEntity != null) == HasEntity;
    }
    public bool HasItem;
    public bool IsWall;
    public bool HasEntity;
}

public enum DG_CHARACTER_TYPE
{
    PLAYER,
    ENEMY,
    QUEST,
    COMPETITIVE,
    ALLY
}
public class DGGenerator : MonoBehaviour, IDebuggable
{
    [SerializeField] private DungeonUIHandler _uiHandler;
    [SerializeField] private Transform _tileContainer;
    [SerializeField] private Transform _entityContainer;
    [SerializeField] private GameObject _playerCharacter;
    [SerializeField] private GameObject _playerCanvases;
    private DGData selectedDungeonData;
    private DungeonFloor _currentFloor;
    private DGPlayer _currentPlayer;
    private List<CharacterBehaviour> _activeParty;
    private CinemachineCamera _virtualCam;
    private DungeonUIHandler _dungeonUI;
    private readonly static int MAX_CHECK_RANDOM = 3000;
    //private readonly static int MAX_CHECK_SEARCH = 500;

    DialogueHandler d;

    public DungeonFloor CurrentFloor { get { return _currentFloor; } }

    private List<CharacterEntry> _tempParty; // Outside of actual party;

    private List<DGEntity> _activeEntities;
    public List<DGEntity> ActiveEntities
    {
        get { return _activeEntities; }
    }
    public List<CharacterBehaviour> ActiveParty
    {
        get { return _activeParty; }
    }

    public List<CharacterEntry> TempParty
    {
        get { return _tempParty; }
    }

    public FloorRoom GetRandomRoom()
    {
        return _currentFloor.rooms[Random.Range(0, _currentFloor.rooms.Count)];
    }
    public TileInfo SearchRandomTileInRoom(FloorRoom room, SearchConditions conditions)
    {
        int checks = 0;
        while (checks < MAX_CHECK_RANDOM)
        {
            var coord = room.GetRandomCoordInRoom();
            var tile = _currentFloor.tiles[_currentFloor.CoordToIndex(coord)];
            if (conditions.AreConditionsMet(tile))
            {
                return tile;
            }
            checks++;
        }
        return null;
    }
    public TileInfo SearchNextAvailableTile(TileInfo origin, SearchConditions conditions, int direction = 0, int dist = 0, int maxDist = 2)
    {
        if (origin == null) return null;
        if (conditions.AreConditionsMet(origin))
            return origin;
        TileInfo foundTile = null;

        var directions = origin.coord.GetDirections();
        foreach (var pt in directions)
        {
            if (DungeonFloor.IsInZ(pt.z) && DungeonFloor.IsInX(pt.x))
            {
                TileInfo tile = _currentFloor.CoordToTileInfo(pt);
                if (conditions.AreConditionsMet(tile))
                {
                    foundTile = tile;
                    return foundTile;
                }
            }
        }

        int currDist = dist + 1;
        if (currDist > maxDist)
            return foundTile;
        if (foundTile == null)
        {
            foreach (var pt in directions)
            {
                if (foundTile != null)
                    break;
                if (DungeonFloor.IsInZ(pt.z) && DungeonFloor.IsInX(pt.x))
                {
                    TileInfo tile = _currentFloor.CoordToTileInfo(pt);
                    foundTile = SearchNextAvailableTile(tile, conditions, currDist, maxDist);
                }
            }
        }
        return foundTile;
    }

    public void FocusCameraOnPlayer()
    {
        if (_currentPlayer == null) return;
        _virtualCam.ForceCameraPosition(_currentFloor.tiles[_currentFloor.CoordToIndex(_currentPlayer.Position)].CoordToPosition(), Quaternion.identity);
        _virtualCam.Follow = _currentPlayer.transform;
    }

    private void RenderCurrentFloor()
    {
        Tileset selectedTileset = Tilesets.Instance.tilesets[(int)selectedDungeonData.defaultTileset];
        for (int i = 0; i < _currentFloor.tiles.Length; i++)
        {
            var newTile = Instantiate(selectedTileset.tileSprite, _currentFloor.tiles[i].CoordToPosition(), Quaternion.identity, _tileContainer);
            if (_currentFloor.tiles[i].isWall)
            {
                newTile.GetComponent<SpriteRenderer>().color = selectedTileset.wallColour;
            }
            else
            {
                newTile.GetComponent<SpriteRenderer>().color = selectedTileset.floorColour;
            }
            if (_currentFloor.tiles[i].structure != null)
            {
                _currentFloor.tiles[i].structure.transform.SetParent(newTile.transform, false);
                _currentFloor.tiles[i].structure.transform.position = _currentFloor.tiles[i].CoordToPosition();
            }
            if (_currentFloor.tiles[i].item != null)
            {
                _currentFloor.tiles[i].item.transform.SetParent(newTile.transform, false);
                _currentFloor.tiles[i].item.transform.position = _currentFloor.tiles[i].CoordToPosition();
            }
        }
    }

    public GameObject AddCharacter(DG_CHARACTER_TYPE charType, TileCoord coord = null)
    {
        if (_currentFloor == null) return null;
        var newCharacter = Instantiate(_playerCharacter, _entityContainer);
        switch(charType) {
            case DG_CHARACTER_TYPE.PLAYER:
                newCharacter.AddComponent<DGPlayer>();
                newCharacter.GetComponent<CharacterBehaviour>().alliance = 0;
                newCharacter.GetComponent<CharacterBehaviour>().allianceIndicator.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 0.25f);
                newCharacter.GetComponent<CharacterBehaviour>().SetUp(GlobalGameManager.Instance.party[0]);
                var newCanvases = Instantiate(_playerCanvases, newCharacter.transform);
                newCanvases.name = "PlayerCanvases";
                break;
            case DG_CHARACTER_TYPE.ALLY:
                newCharacter.AddComponent<DGEntity>();
                Destroy(newCharacter.GetComponent<PlayerInput>());
                var allyNPC = newCharacter.AddComponent<DGNPC>();
                allyNPC.main = AIAlly.Instance;
                newCharacter.GetComponent<CharacterBehaviour>().alliance = 0;
                newCharacter.GetComponent<CharacterBehaviour>().allianceIndicator.GetComponent<SpriteRenderer>().color = new Color(1, 1, 0, 0.25f);
                break;
            case DG_CHARACTER_TYPE.COMPETITIVE:
                newCharacter.AddComponent<DGEntity>();
                Destroy(newCharacter.GetComponent<PlayerInput>());
                var dgnpc = newCharacter.AddComponent<DGNPC>();
                dgnpc.main = AICompetitive.Instance;
                newCharacter.GetComponent<CharacterBehaviour>().allianceIndicator.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.25f);
                break;
            case DG_CHARACTER_TYPE.QUEST:
                newCharacter.AddComponent<DGEntity>();
                Destroy(newCharacter.GetComponent<PlayerInput>());
                var npc = newCharacter.AddComponent<DGNPC>();
                npc.main = AIWander.Instance;
                npc.isQuestTarget = true;
                newCharacter.GetComponent<CharacterBehaviour>().alliance = -1;
                newCharacter.GetComponent<CharacterBehaviour>().allianceIndicator.GetComponent<SpriteRenderer>().color = new Color(0, 0.64f, 1.0f, 0.25f);
                var interactable = newCharacter.AddComponent<DGInteractable>();
                interactable.DestroyOnInteract = false;
                interactable.CanInteractWithAction = true;
                interactable.interaction = DIRescue.Instance;
                interactable.transform.localScale /= TileInfo.tileScale;
                break;
            default:
            case DG_CHARACTER_TYPE.ENEMY:
                newCharacter.AddComponent<DGEntity>();
                Destroy(newCharacter.GetComponent<PlayerInput>());
                var npcMod = newCharacter.AddComponent<DGNPC>();
                npcMod.main = AIEnemy.Instance;
                newCharacter.GetComponent<CharacterBehaviour>().alliance = 1;
                newCharacter.GetComponent<CharacterBehaviour>().allianceIndicator.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.25f);
                break;
                
        }

        FloorRoom room = GetRandomRoom();
        if (coord == null)
        {
            TileCoord randCoord = null;
            int searches = 0;
            int searchMax = 10000;
            while (searches < searchMax)
            {
                searches++;
                room = GetRandomRoom();
                randCoord = room.GetRandomCoordInRoom();
                var tile = CurrentFloor.CoordToTileInfo(randCoord);
                if (tile.occupyingEntity == null)
                    break;
            }
            newCharacter.GetComponent<DGEntity>().Set(_currentFloor, randCoord);

        } else
        {
            newCharacter.GetComponent<DGEntity>().Set(_currentFloor, coord);
        }
        newCharacter.GetComponent<DGEntity>().Warp(newCharacter.GetComponent<DGEntity>().Position);
        return newCharacter;
    }

    [Tooltip("This function is used for after the map has been rendered.")]
    public void InsertItem(GameObject item, TileInfo tile)
    {
        tile.AddItem(item.GetComponent<DGInteractable>());
        Transform tileObj = _tileContainer.GetChild(_currentFloor.CoordToIndex(tile.coord));
        tile.item.transform.SetParent(tileObj, false);
        tile.item.transform.position = tile.CoordToPosition();
    }
    private void PlacePlayer(TileCoord point = null)
    {
        if (_currentFloor == null) return;
        FloorRoom room = GetRandomRoom();
        _currentPlayer.Set(_currentFloor, point != null ? point : room.GetRandomCoordInRoom());
        _currentPlayer.Warp(_currentPlayer.Position);
    }

    private void SpawnPlayer(TileCoord point = null)
    {
        var newPlayer = AddCharacter(DG_CHARACTER_TYPE.PLAYER, point);
        _currentPlayer = newPlayer.GetComponent<DGPlayer>();
        _activeEntities.Add(_currentPlayer);
        _activeParty.Add(newPlayer.GetComponent<CharacterBehaviour>());
        _dungeonUI.RegisterPlayer(_currentPlayer);

        _virtualCam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        if (_virtualCam)
        {
            _virtualCam.ForceCameraPosition(_currentFloor.tiles[_currentFloor.CoordToIndex(_currentPlayer.Position)].CoordToPosition(), Quaternion.identity);
            _virtualCam.Follow = newPlayer.transform;
        }
        if (SaveDataManager.Instance.QuickSaveFile != null)
        {
            var file = SaveDataManager.Instance.QuickSaveFile;
            var playerData = file.floorData.activeParty[0];
            var entity = _currentPlayer.GetComponent<DGEntity>();
            entity.Warp(playerData.position);
            entity.FaceDir = playerData.direction;
            var cb = _currentPlayer.GetComponent<CharacterBehaviour>();
            playerData.LoadStats(cb);
        }
        _currentPlayer.gameObject.name = newPlayer.GetComponent<CharacterBehaviour>().character.characterName;
    }

    private void SpawnParty()
    {
        if ((GlobalGameManager.Instance.party.Count + _tempParty.Count) < 2)
            return;
        var file = SaveDataManager.Instance.QuickSaveFile;
        for (int i = 1; i < GlobalGameManager.Instance.party.Count; i++)
        {
            var spawnTile = SearchNextAvailableTile(_currentFloor.CoordToTileInfo(_currentPlayer.Position), SearchConditions.New(), 0, 0, 10);
            var newPartyMember = AddCharacter(DG_CHARACTER_TYPE.ALLY, spawnTile.coord);
            newPartyMember.GetComponent<CharacterBehaviour>().SetUp(GlobalGameManager.Instance.party[i]);
            _activeEntities.Add(newPartyMember.GetComponent<DGEntity>());
            _activeParty.Add(newPartyMember.GetComponent<CharacterBehaviour>());

            if (file != null)
            {
                for (int preIndex = 1; preIndex < file.floorData.activeParty.Count; preIndex++) // Loop through activeParty to find my data
                {
                    if (file.floorData.activeParty[preIndex].character.Equals(newPartyMember.GetComponent<CharacterBehaviour>().character)) // if the character in question is the same as the one we had at the start
                    {
                        var memberData = file.floorData.activeParty[preIndex];
                        var entity = newPartyMember.GetComponent<DGEntity>();
                        entity.Warp(memberData.position);
                        entity.FaceDir = memberData.direction;

                        var cb = newPartyMember.GetComponent<CharacterBehaviour>();
                        memberData.LoadStats(cb);
                        break;
                    }
                }
            }

            newPartyMember.gameObject.name = newPartyMember.GetComponent<CharacterBehaviour>().character.characterName;
        }
        for (int i = 0; i < _tempParty.Count; i++)
        {
            int trueIndex = i + GlobalGameManager.Instance.party.Count;
            var spawnTile = SearchNextAvailableTile(_currentFloor.CoordToTileInfo(_currentPlayer.Position), SearchConditions.New(), 0, 0, 10);
            var newPartyMember = AddCharacter(DG_CHARACTER_TYPE.ALLY, spawnTile.coord);
            newPartyMember.GetComponent<CharacterBehaviour>().SetUp(_tempParty[i]);
            _activeEntities.Add(newPartyMember.GetComponent<DGEntity>());
            _activeParty.Add(newPartyMember.GetComponent<CharacterBehaviour>());

            if (file != null)
            {
                if (file.floorData.activeParty.Count > trueIndex)
                {
                    for (int preIndex = 1; preIndex < file.floorData.activeParty.Count; preIndex++) // Loop through activeParty to find my data
                    {
                        if (file.floorData.activeParty[preIndex].character.Equals(newPartyMember.GetComponent<CharacterBehaviour>().character)) // if the character in question is the same as the one we had at the start
                    {
                            var memberData = file.floorData.activeParty[preIndex];
                            var entity = newPartyMember.GetComponent<DGEntity>();
                            entity.Warp(memberData.position);
                            entity.FaceDir = memberData.direction;

                            var cb = newPartyMember.GetComponent<CharacterBehaviour>();
                            memberData.LoadStats(cb);
                            break;
                        }
                    }
                }
            }
            newPartyMember.gameObject.name = newPartyMember.GetComponent<CharacterBehaviour>().character.characterName;
        }

        List<CharacterBehaviour> toRemove = new List<CharacterBehaviour>();
        if (file != null && _activeParty.Count > 1)
        {
            for (int i = 1; i < _activeParty.Count; i++) // Loop through party members, check if they are alive (their preData exists and their activeParty data exists)
            {
                bool activeFound = false;
                for (int j = 1; j < file.floorData.activeParty.Count; j++)
                {
                    if (file.floorData.activeParty[j].character.Equals(_activeParty[i].character))
                    {
                        activeFound = true;
                        break;
                    }
                }
                if (!activeFound)
                {
                    _activeParty[i].health = 0;
                    toRemove.Add(_activeParty[i]);
                }
            }
        }
        _dungeonUI.RegisterParty(ActiveParty);
        for (int i = toRemove.Count - 1; i >= 0; i--) {
            _activeEntities.Remove(toRemove[i].GetComponent<DGEntity>());
            _activeParty.Remove(toRemove[i]);
            Destroy(toRemove[i].gameObject);
        }
    }

    private void PlaceParty()
    {
        if (_currentFloor == null || _currentPlayer == null || ActiveParty.Count < 2) return;
        for (int i = 1; i < ActiveParty.Count; i++)
        {
            var spawnTile = SearchNextAvailableTile(_currentFloor.CoordToTileInfo(_currentPlayer.Position), SearchConditions.New(), 0, 0, 10);
            ActiveParty[i].GetComponent<DGEntity>().Set(_currentFloor, spawnTile != null ? spawnTile.coord : GetRandomRoom().GetRandomCoordInRoom());
            ActiveParty[i].GetComponent<DGEntity>().Warp(ActiveParty[i].GetComponent<DGEntity>().Position);
        }
    }

    public void AddTempParty(CHARACTER_ENUM character, int startingLevel)
    {
        var newCharacter = CharacterEntry.Create(character, startingLevel);
        _tempParty.Add(newCharacter);
    }

    public GameObject SpawnNPC(DG_CHARACTER_TYPE charType, CharacterEntry characterData, TileCoord point = null)
    {
        var newCharacter = AddCharacter(charType, point);
        _activeEntities.Add(newCharacter.GetComponent<DGEntity>());
        newCharacter.GetComponent<CharacterBehaviour>().SetUp(characterData);
        return newCharacter;
    }

    public List<DGEntity> SpawnCompetitors(QuestCompetitor competitor, int questIndex, int compIndex)
    {
        var file = SaveDataManager.Instance.QuickSaveFile;
        var party = competitor.party;
        FloorRoom room = GetRandomRoom();
        TileInfo point = SearchRandomTileInRoom(room, SearchConditions.New());
        var newList = new List<DGEntity>();
        for (int i = 0; i < party.Count; i++)
        {
            var character = party[i];
            var newCharacter = AddCharacter(DG_CHARACTER_TYPE.COMPETITIVE, point != null ? point.coord : null);
            newCharacter.gameObject.name = character.characterName;
            newCharacter.GetComponent<DGNPC>().associatedCompetitor = competitor;
            newCharacter.GetComponent<CharacterBehaviour>().alliance = questIndex;
            _activeEntities.Add(newCharacter.GetComponent<DGEntity>());
            
            newCharacter.GetComponent<CharacterBehaviour>().SetUp(character);
            newList.Add(newCharacter.GetComponent<DGEntity>());

            if (file != null)
            {
                foreach (var q in file.questCompetitors)
                {
                    if (q.associatedID == competitor.associatedQuest.questID)
                    {
                        var data = q.competitors[compIndex];
                        newCharacter.GetComponent<DGEntity>().Warp(data.partySpawned[i].position);
                        newCharacter.GetComponent<DGEntity>().FaceDir = data.partySpawned[i].direction;
                        data.partySpawned[i].LoadStats(newCharacter.GetComponent<CharacterBehaviour>());
                        break;
                    }
                }
            }
        }
        competitor.partySpawned = newList;
        return newList;
    }

    private void Start()
    {
        _activeEntities = new List<DGEntity>();
        d = GlobalCanvasManager.Instance.DialogueHandler;
        selectedDungeonData = GlobalGameManager.Instance.selectedDungeon;
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _activeParty = new List<CharacterBehaviour>();
        _tempParty = new List<CharacterEntry>();
    }

    public void ClearFloor()
    {
        _uiHandler.ClearTurnLog();
        for (int i = _tileContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_tileContainer.GetChild(i).gameObject);
        }
        for (int i = _entityContainer.childCount - 1; i >= 1; i--)
        {
            if (!ActiveParty.Contains(_entityContainer.GetChild(i).GetComponent<CharacterBehaviour>()))
            {
                _activeEntities.Remove(_entityContainer.GetChild(i).GetComponent<DGEntity>());
                Destroy(_entityContainer.GetChild(i).gameObject);
            }
        }
    }

    public void RenameEntities()
    {
        Dictionary<string, CharacterBehaviour> firstInstances = new Dictionary<string, CharacterBehaviour>();
        Dictionary<string, int> charCount = new Dictionary<string, int>();
        for (int i = 0; i < _activeEntities.Count; i++)
        {
            var cb = _activeEntities[i].GetComponent<CharacterBehaviour>();
            var charProfile = CharacterProfiles.Instance.characterProfiles[(int)cb.character.associatedCharacter];

            if (!charCount.ContainsKey(charProfile.characterName))
                charCount.Add(charProfile.characterName, 0);
            charCount[charProfile.characterName]++;

            if (!firstInstances.ContainsKey(charProfile.characterName))
            {
                firstInstances.Add(charProfile.characterName, cb);
                cb.gameObject.name = charProfile.characterName;
                cb.character.characterName = cb.gameObject.name;
            } else
            {
                firstInstances[charProfile.characterName].name = charProfile.characterName + " 1";
                cb.gameObject.name = charProfile.characterName + " " + charCount[charProfile.characterName];
                cb.character.characterName = cb.gameObject.name;
            }
        }
    }

    public void NewFloor(DGSeed givenSeed = null)
    {
        ClearFloor();
        if (SaveDataManager.Instance.QuickSaveFile == null)
        {
            DGSeed selectedSeed = null;
            if (givenSeed)
            {
                selectedSeed = givenSeed;
            }
            else
            {
                selectedSeed = selectedDungeonData.floorSeed;
            }
            _currentFloor = selectedSeed.Generate(selectedDungeonData);
            RenderCurrentFloor();
            TileCoord point = null;
            if (selectedSeed != null && selectedSeed is StaticSeed)
            {
                var seed = selectedSeed as StaticSeed;
                point = new TileCoord(seed.playerSpawnX, seed.playerSpawnY);
            }
            if (_currentPlayer == null)
            {
                SpawnPlayer(point);
                SpawnParty();
            }
            else
            {
                PlacePlayer(point);
                PlaceParty();
            }
            if (selectedSeed != null)
            {
                selectedSeed.AddItems(this);
                selectedSeed.AddEnemies(this);
            }
            RenameEntities();
        } else
        {
            var file = SaveDataManager.Instance.QuickSaveFile;
            _currentFloor = file.floorData.ExtractFloor();
            RenderCurrentFloor();

            for (int i = 0; i < file.floorData.tempParty.Count; i++)
            {
                var loadChar = file.floorData.tempParty[i].Extract();
                _tempParty.Add(loadChar);
            }

            SpawnPlayer();
            SpawnParty();

            file.floorData.AddItems(this);
            file.floorData.AddEnemies(this);
        }
    }

    public void Update()
    {
        DebugControls();
    }

    public void DebugControls()
    {
        if (!DebugTools.Instance.DungeonDebugOn) return;
        if (Input.GetKeyDown(KeyCode.L))
        {
            for (int i = _tileContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_tileContainer.GetChild(i).gameObject);
            }
            _currentFloor = selectedDungeonData.floorSeed.Generate(selectedDungeonData);
            RenderCurrentFloor();
            PlacePlayer();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            d.PromptSequence(DebugTools.Instance.testDialogueSequence);
        }
    }
}
