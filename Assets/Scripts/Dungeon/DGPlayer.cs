using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DGPlayer : DGEntity
{
    public System.Action<CHARACTER_STAT, int, int> OnLeaderStatChanged;
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
        get { return !(_performingAction || _dgGameManager.CurrentEntityTurn() != this || _dungeonUI.menu.IsOpen); }
    }

    private new void Update()
    {
        base.Update();

        if (!CanControl) return;
        _orientationIndicator.color = new Color(_indicatorColor.r, _indicatorColor.g, _indicatorColor.b, _inputManager.actions["Anchor"].IsPressed() ? 1.0f : _transparency);
        _orientationRotator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(faceDir.z, faceDir.x) * Mathf.Rad2Deg - 90.0f);

        if (_inputManager.actions["Accept"].WasPressedThisFrame())
        {
            DefaultAttack.Instance.Perform(_cb);
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
        List<TileCoord> pattern = new List<TileCoord>();
        pattern.Add(Position);
        pattern.Add(Position.North);
        pattern.Add(Position.North.North);
        pattern.Add(Position.Northeast);
        pattern.Add(Position.East);
        pattern.Add(Position.East.East);
        pattern.Add(Position.Southeast);
        pattern.Add(Position.South);
        pattern.Add(Position.South.South);
        pattern.Add(Position.Southwest);
        pattern.Add(Position.West);
        pattern.Add(Position.West.West);
        pattern.Add(Position.Northwest);
        foreach (TileCoord coord in pattern) { 
            if (DungeonFloor.IsInX(coord.x) && DungeonFloor.IsInZ(coord.z))
            {
                _dungeonGen.CurrentFloor.CoordToTileInfo(coord).hasBeenDiscovered = true;
            }
        }
        pattern.Clear();
    }
}
