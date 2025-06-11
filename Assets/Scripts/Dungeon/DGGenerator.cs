using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using Unity.VisualScripting;
using JetBrains.Annotations;

public struct SearchConditions {
    public static SearchConditions New(bool hasItem = false, bool isWall = false)
    {
        SearchConditions conditions = new SearchConditions();
        conditions.HasItem = hasItem;
        conditions.IsWall = isWall;
        return conditions;
    }
    public bool AreConditionsMet(TileInfo tile)
    {
        return ((tile.item != null) == HasItem && tile.isWall == IsWall);
    }
    public bool HasItem;
    public bool IsWall;
}

public class DGGenerator : MonoBehaviour, IDebuggable
{
    [SerializeField] private DungeonUIHandler _uiHandler;
    [SerializeField] private Transform _tileContainer;
    [SerializeField] private Transform _entityContainer;
    [SerializeField] private GameObject _playerCharacter;
    private DGData selectedDungeonData;
    private DungeonFloor _currentFloor;
    private DGPlayer _currentPlayer;
    private List<CharacterBehaviour> _activeParty;
    private CinemachineCamera _virtualCam;
    private DungeonUIHandler _dungeonUI;
    private readonly static int MAX_CHECK_RANDOM = 3000;
    //private readonly static int MAX_CHECK_SEARCH = 500;

    DialogueHandler d;

    private List<DGEntity> _activeEntities;
    public List<DGEntity> ActiveEntities
    {
        get { return _activeEntities; }
    }
    public List<CharacterBehaviour> ActiveParty
    {
        get { return _activeParty; }
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
        if (conditions.AreConditionsMet(origin))
            return origin;
        TileInfo foundTile = null;
        int currDist = dist + 1;
        if (currDist > maxDist)
            return foundTile;
        if (direction == 0)
        {
            TileCoord upCoord = origin.coord.North;
            if (DungeonFloor.IsInZ(upCoord.z) && DungeonFloor.IsInX(upCoord.x))
            {
                TileInfo tile = _currentFloor.tiles[_currentFloor.CoordToIndex(upCoord)];
                if (conditions.AreConditionsMet(tile))
                {
                    foundTile = tile;
                } else
                {
                    TileInfo upSearch = SearchNextAvailableTile(tile, conditions, 0, currDist, maxDist);
                    if (upSearch != null)
                    {
                        foundTile = upSearch;
                        return foundTile;
                    }
                    TileInfo rightSearch = SearchNextAvailableTile(tile, conditions, 1, currDist, maxDist);
                    if (rightSearch != null)
                    {
                        foundTile = rightSearch;
                        return foundTile;
                    }
                    TileInfo leftSearch = SearchNextAvailableTile(tile, conditions, 3, currDist, maxDist);
                    if (leftSearch != null)
                    {
                        foundTile = leftSearch;
                        return foundTile;
                    }
                }
            }
        }
        if (direction == 1) {
            TileCoord rightCoord = origin.coord.East;
            if (DungeonFloor.IsInZ(rightCoord.z) && DungeonFloor.IsInX(rightCoord.x))
            {
                TileInfo tile = _currentFloor.tiles[_currentFloor.CoordToIndex(rightCoord)];
                if (conditions.AreConditionsMet(tile))
                {
                    foundTile = tile;
                }
                else
                {
                    TileInfo upSearch = SearchNextAvailableTile(tile, conditions, 0, currDist, maxDist);
                    if (upSearch != null)
                    {
                        foundTile = upSearch;
                        return foundTile;
                    }
                    TileInfo rightSearch = SearchNextAvailableTile(tile, conditions, 1, currDist, maxDist);
                    if (rightSearch != null)
                    {
                        foundTile = rightSearch;
                        return foundTile;
                    }
                    TileInfo downSearch = SearchNextAvailableTile(tile, conditions, 2, currDist, maxDist);
                    if (downSearch != null)
                    {
                        foundTile = downSearch;
                        return foundTile;
                    }
                }
            }
        }
        if (direction == 2)
        {
            TileCoord downCoord = origin.coord.South;
            if (DungeonFloor.IsInZ(downCoord.z) && DungeonFloor.IsInX(downCoord.x))
            {
                TileInfo tile = _currentFloor.tiles[_currentFloor.CoordToIndex(downCoord)];
                if (conditions.AreConditionsMet(tile))
                {
                    foundTile = tile;
                }
                else
                {
                    TileInfo leftSearch = SearchNextAvailableTile(tile, conditions, 3, currDist, maxDist);
                    if (leftSearch != null)
                    {
                        foundTile = leftSearch;
                        return foundTile;
                    }
                    TileInfo rightSearch = SearchNextAvailableTile(tile, conditions, 1, currDist, maxDist);
                    if (rightSearch != null)
                    {
                        foundTile = rightSearch;
                        return foundTile;
                    }
                    TileInfo downSearch = SearchNextAvailableTile(tile, conditions, 2, currDist, maxDist);
                    if (downSearch != null)
                    {
                        foundTile = downSearch;
                        return foundTile;
                    }
                }
            }
        }
        if (direction == 3)
        {
            TileCoord leftCoord = origin.coord.West;
            if (DungeonFloor.IsInZ(leftCoord.z) && DungeonFloor.IsInX(leftCoord.x))
            {
                TileInfo tile = _currentFloor.tiles[_currentFloor.CoordToIndex(leftCoord)];
                if (conditions.AreConditionsMet(tile))
                {
                    foundTile = tile;
                }
                else
                {
                    TileInfo leftSearch = SearchNextAvailableTile(tile, conditions, 3, currDist, maxDist);
                    if (leftSearch != null)
                    {
                        foundTile = leftSearch;
                        return foundTile;
                    }
                    TileInfo upSearch = SearchNextAvailableTile(tile, conditions, 0, currDist, maxDist);
                    if (upSearch != null)
                    {
                        foundTile = upSearch;
                        return foundTile;
                    }
                    TileInfo downSearch = SearchNextAvailableTile(tile, conditions, 2, currDist, maxDist);
                    if (downSearch != null)
                    {
                        foundTile = downSearch;
                        return foundTile;
                    }
                }
            }
        }
        return foundTile;
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

    public GameObject AddCharacter(bool isPlayer, TileCoord coord = null)
    {
        if (_currentFloor == null) return null;
        var newCharacter = Instantiate(_playerCharacter, _entityContainer);
        if (isPlayer)
        {
            newCharacter.AddComponent<DGPlayer>();
            newCharacter.GetComponent<CharacterBehaviour>().alliance = CharacterBehaviour.ALLIANCE.TEAM_1;
        }
        else
        {
            newCharacter.AddComponent<DGEntity>();
            Destroy(newCharacter.GetComponent<PlayerInput>());
            newCharacter.AddComponent<DGNPC>();
            newCharacter.GetComponent<CharacterBehaviour>().alliance = CharacterBehaviour.ALLIANCE.TEAM_2;
        }

        FloorRoom room = _currentFloor.rooms[Random.Range(0, _currentFloor.rooms.Count)];
        newCharacter.GetComponent<DGEntity>().Set(_currentFloor, coord == null ? room.GetRandomCoordInRoom() : coord);
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
    private void PlacePlayer()
    {
        if (_currentFloor == null) return;
        FloorRoom room = _currentFloor.rooms[Random.Range(0, _currentFloor.rooms.Count)];
        _currentPlayer.Set(_currentFloor, room.GetRandomCoordInRoom());
        _currentPlayer.Warp(_currentPlayer.Position);
    }

    private void SpawnPlayer()
    {
        var newPlayer = AddCharacter(true);
        _currentPlayer = newPlayer.GetComponent<DGPlayer>();
        _activeEntities.Add(_currentPlayer);
        _activeParty.Add(newPlayer.GetComponent<CharacterBehaviour>());
        newPlayer.GetComponent<CharacterBehaviour>().SetUp(GlobalGameManager.Instance.party[0]);
        _dungeonUI.RegisterPlayer(_currentPlayer);
        PlacePlayer();
        _virtualCam = GameObject.FindFirstObjectByType<CinemachineCamera>();
        if (_virtualCam)
        {
            _virtualCam.ForceCameraPosition(_currentFloor.tiles[_currentFloor.CoordToIndex(_currentPlayer.Position)].CoordToPosition(), Quaternion.identity);
            _virtualCam.Follow = newPlayer.transform;
        }
    }

    public void SpawnNPC(CharacterEntry characterData)
    {
        var newCharacter = AddCharacter(false);
        _activeEntities.Add(newCharacter.GetComponent<DGEntity>());
        newCharacter.GetComponent<CharacterBehaviour>().SetUp(characterData);
    }



    private void Start()
    {
        _activeEntities = new List<DGEntity>();
        d = GlobalCanvasManager.Instance.DialogueHandler;
        selectedDungeonData = GlobalGameManager.Instance.selectedDungeon;
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _activeParty = new List<CharacterBehaviour>();
    }

    private void ClearFloor()
    {
        _uiHandler.ClearTurnLog();
        for (int i = _tileContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_tileContainer.GetChild(i).gameObject);
        }
        for (int i = _entityContainer.childCount - 1; i >= 1; i--)
        {
            if (_entityContainer.GetChild(i) != _currentPlayer.gameObject)
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
            } else
            {
                firstInstances[charProfile.characterName].name = charProfile.characterName + " 1";
                cb.gameObject.name = charProfile.characterName + " " + charCount[charProfile.characterName];
            }
        }
    }

    public void NewFloor()
    {
        ClearFloor();
        _currentFloor = selectedDungeonData.floorSeed.Generate(selectedDungeonData);
        RenderCurrentFloor();
        if (_currentPlayer == null)
        {
            SpawnPlayer();
        } else
        {
            PlacePlayer();
        }
        selectedDungeonData.floorSeed.AddEnemies(this);
        RenameEntities();
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
