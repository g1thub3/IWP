using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DGPlayer : DGEntity
{
    public System.Action<CHARACTER_STAT, int, int> OnLeaderStatChanged;
    public System.Action OnLeaderLevelChanged;
    private PlayerInput _inputManager;
    private CharacterBehaviour _cb;
    private Transform _orientationRotator;
    private SpriteRenderer _orientationIndicator;

    private static readonly float _transparency = 35.0f / 255.0f;
    private Color _indicatorColor;

    private new void Start()
    {
        base.Start();
        _inputManager = GetComponent<PlayerInput>();
        _cb = GetComponent<CharacterBehaviour>();
        _orientationRotator = transform.Find("Rotator");
        _orientationIndicator = _orientationRotator.GetComponentInChildren<SpriteRenderer>();
        _orientationIndicator.enabled = true;
        _indicatorColor = _orientationIndicator.color;

    }

    public bool CanControl
    {
        get { return !(_performingAction || _dgGameManager.CurrentEntityTurn() != this || _dungeonUI.menu.IsOpen || GlobalCanvasManager.Instance.IsInteractionActive); }
    }

    private new void Update()
    {
        base.Update();

        if (!CanControl) return;
        _orientationIndicator.color = new Color(_indicatorColor.r, _indicatorColor.g, _indicatorColor.b, _inputManager.actions["Anchor"].IsPressed() ? 1.0f : _transparency);
        _orientationRotator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(faceDir.z, faceDir.x) * Mathf.Rad2Deg - 90.0f);

        if (_inputManager.actions["Accept"].WasPressedThisFrame())
        {
            if (!InteractAction())
            {
                _cb.PerformMove(DefaultAttack.Instance);
            }
            return;
        }

        int x, y;
        bool hasInput = false;
        x = y = 0;

        if (_inputManager.actions["Up"].IsPressed())
        {
            hasInput = true;
            y++;
        }
        if (_inputManager.actions["Right"].IsPressed())
        {
            hasInput = true;
            x++;
        }
        if (_inputManager.actions["Down"].IsPressed())
        {
            hasInput = true;
            y--;
        }
        if (_inputManager.actions["Left"].IsPressed())
        {
            hasInput = true;
            x--;
        }

        if (!_inputManager.actions["Anchor"].IsPressed() || _inputManager.actions["Anchor"].WasReleasedThisFrame())
        {
            if (Move(x, y))
                Discover();
        } else if (_inputManager.actions["Anchor"].IsPressed() && hasInput)
        {
            faceDir.x = x;
            faceDir.z = y;
            if (faceDir.z == 1)
                NumToDir(2);
            if (faceDir.x == 1)
                NumToDir(3);
            if (faceDir.z == -1)
                NumToDir(0);
            if (faceDir.x == -1)
                NumToDir(1);
        }
    }

    private void Discover()
    {
        if (_currRoom != null)
        {
            for (int i = -1; i <= _currRoom.length; i++)
            {
                for (int j = -1; j <= _currRoom.height; j++)
                {
                    TileCoord coord = new TileCoord(_currRoom.origin.x + i, _currRoom.origin.z + j);
                    if (DungeonFloor.IsInX(coord.x) && DungeonFloor.IsInZ(coord.z))
                    {
                        _dungeonGen.CurrentFloor.CoordToTileInfo(coord).hasBeenDiscovered = true;
                    }
                }
            }
            return;
        }

        Floor.ClearSearch();

        List<TileCoord> pattern = new List<TileCoord>();

        foreach (var tile in Floor.nonWallTiles)
        {
            float dist = tile.coord.DistanceSquared(Position);
            if (dist <= GlobalGameManager.Instance.party[0].viewDistance)
                pattern.Add(tile.coord);
        }

        //TileCoord prev = null;
        //TileCoord curr = Position;

        //bool isSearching = true;
        //while (isSearching)
        //{
        //    TileCoord foundDirection = null;
        //    var directions = curr.GetDirections();
        //    foreach (var direction in directions) {
        //        if (!(DungeonFloor.IsInX(direction.x) && DungeonFloor.IsInZ(direction.z))) continue;
        //        if (Floor.tilePathPoints[Floor.CoordToIndex(curr)].hasSearched) continue;
        //        float dist = direction.DistanceSquared(Position);
        //        if (dist > GlobalGameManager.Instance.party[0].viewDistance) continue;
        //        foundDirection = direction;
        //        break;
        //    }
        //    if (foundDirection != null)
        //    {
        //        prev = curr;
        //        curr = foundDirection;
        //        continue;
        //    }

        //    Floor.tilePathPoints[Floor.CoordToIndex(curr)].hasSearched = true;
        //    pattern.Add(curr);
        //    // NO DIRECTIONS LEFT
        //    if (prev != null)
        //    {
        //        if (Floor.tilePathPoints[Floor.CoordToIndex(prev)].hasSearched)
        //        {
        //            isSearching = false;
        //        } else
        //        {
        //            Floor.tilePathPoints[Floor.CoordToIndex(curr)].hasSearched = true;
        //            curr = prev;
        //        }
        //    } else
        //    {
        //        isSearching = false;
        //    }
        //}

        foreach (TileCoord coord in pattern) { 
            if (DungeonFloor.IsInX(coord.x) && DungeonFloor.IsInZ(coord.z))
            {
                _dungeonGen.CurrentFloor.CoordToTileInfo(coord).hasBeenDiscovered = true;
            }
        }
        pattern.Clear();
    }
}
