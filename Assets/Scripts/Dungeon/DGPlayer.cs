using System.Collections.Generic;
using TMPro;
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
    private bool _selectingMove;
    private int _moveSelection;


    private static readonly float _transparency = 35.0f / 255.0f;
    private Color _indicatorColor;

    [SerializeField] DefaultAttack _defaultAttackInstance;

    Canvas _moveCanvas, _attackCanvas;
    Transform _moveList;

    private void LoadMoveOptions()
    {
        CombatMove[] selectedMoves = new CombatMove[5];
        for (int i = -2; i <= 2; i++)
        {
            if (_cb.AvailableMoves.Count == 0)
            {
                selectedMoves[i + 2] = _defaultAttackInstance;
                continue;
            }
            int index = i + _moveSelection;
            while (index < 0)
            {
                index += _cb.AvailableMoves.Count;
            }
            while (index >= _cb.AvailableMoves.Count)
            {
                index -= _cb.AvailableMoves.Count;
            }
            selectedMoves[i + 2] = _cb.AvailableMoves[index];
        }
        for (int i = 0; i < selectedMoves.Length; i++) {
            var moveObj = _moveList.GetChild(i);
            var moveName = moveObj.Find("MoveName");
            var costText = moveObj.Find("CostText");
            moveName.GetComponent<TMP_Text>().text = selectedMoves[i].moveName;
            if (selectedMoves[i].consumptionType == CHARACTER_STAT.ENERGY)
            {
                costText.GetComponent<TMP_Text>().text = selectedMoves[i].energyRequirement + " EN";
                if (ColorUtility.TryParseHtmlString("#FFAD69", out Color enclr))
                    costText.GetComponent<TMP_Text>().color = enclr;
            } else if (selectedMoves[i].consumptionType == CHARACTER_STAT.MANA)
            {
                costText.GetComponent<TMP_Text>().text = selectedMoves[i].energyRequirement + " MN";
                if (ColorUtility.TryParseHtmlString("#B99BE0", out Color mnclr))
                    costText.GetComponent<TMP_Text>().color = mnclr;
            }
        }
    }

    private new void Start()
    {
        base.Start();
        _inputManager = GetComponent<PlayerInput>();
        _cb = GetComponent<CharacterBehaviour>();
        _orientationRotator = transform.Find("Rotator");
        _orientationIndicator = _orientationRotator.GetComponentInChildren<SpriteRenderer>();
        _orientationIndicator.enabled = true;
        _indicatorColor = _orientationIndicator.color;

        _moveCanvas = transform.Find("PlayerCanvases/MoveSelectionCanvas").GetComponent<Canvas>();
        _attackCanvas = transform.Find("PlayerCanvases/AttackLandCanvas").GetComponent<Canvas>();
        _moveList = _moveCanvas.transform.Find("MoveList");
        _selectingMove = false;
        _moveSelection = 0;
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
        _orientationRotator.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(FaceDir.z, FaceDir.x) * Mathf.Rad2Deg - 90.0f);

        if (_selectingMove)
        {
            if (_inputManager.actions["Up"].WasPressedThisFrame())
            {
                _moveSelection--;
                if (_moveSelection < 0)
                    _moveSelection = _cb.AvailableMoves.Count - 1;
                LoadMoveOptions();
            }
            if (_inputManager.actions["Down"].WasPressedThisFrame())
            {
                _moveSelection++;
                if (_moveSelection >= _cb.AvailableMoves.Count)
                    _moveSelection = 0;
                LoadMoveOptions();
            }
            if (_inputManager.actions["Accept"].WasPressedThisFrame())
            {
                _cb.PerformMove(_cb.AvailableMoves[_moveSelection]);
                _selectingMove = false;
                _attackCanvas.gameObject.SetActive(false);
                _moveCanvas.gameObject.SetActive(false);
            }
            if (_inputManager.actions["Decline"].WasPressedThisFrame())
            {
                _selectingMove = false;
                _attackCanvas.gameObject.SetActive(false);
                _moveCanvas.gameObject.SetActive(false);
            }
            return;
        }
        if (_inputManager.actions["Accept"].WasPressedThisFrame())
        {
            if (!InteractAction())
            {
                _selectingMove = true;
                _attackCanvas.transform.localPosition = new Vector2(faceDir.x, faceDir.z);
                _moveCanvas.transform.localPosition = new Vector2(-faceDir.x, -faceDir.z);
                _attackCanvas.gameObject.SetActive(true);
                _moveCanvas.gameObject.SetActive(true);
                LoadMoveOptions();
                return;
            }
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
            FaceDirection(x, y);
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
