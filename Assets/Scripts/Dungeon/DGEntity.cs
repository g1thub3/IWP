using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Splines;

public class TilePathPoint {
    public TileInfo tile;
    public float searchScore;
    public bool hasSearched;
    public TilePathPoint(TileInfo tile)
    {
        searchScore = 0;
        hasSearched = false;
    }
    public void Reset()
    {
        searchScore = 0;
        hasSearched = false;
    }
}

public class DGEntity : DGObject
{
    [SerializeField]
    private CharacterBehaviour _characterBehaviour;
    [SerializeField] private float _moveTime = 0.4f;
    protected Animator _animator;
    TileInfo occupyingTile;
    protected DGGameManager _dgGameManager;
    protected DungeonUIHandler _dungeonUI;
    protected bool _performingAction;

    public TileCoord faceDir;

    public bool IsPerformingAction
    {
        get { return _performingAction; }
        set { _performingAction = value; }
    }
    private IEnumerator MoveCoroutine(Vector2 original, Vector2 destined)
    {
        _animator.SetBool("isWalking", true);
        float t = 0;
        while (t < _moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(original, destined, t/_moveTime);   
            yield return new WaitForEndOfFrame();
        }
        transform.position = destined;
        _animator.SetBool("isWalking", false);

        TileInfo tile = floor.tiles[floor.CoordToIndex(position)];
        if (tile.item != null)
        {
            tile.item.OnInteract(this, floor);
            while (tile.item.interaction.InteractionInProgress)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        if (tile.structure != null)
        {
            tile.structure.OnInteract(this, floor);
            while (tile.structure.interaction.InteractionInProgress)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        _performingAction = false;
    }

    public bool Move(int right, int up) // Limited movement
    {
        right = Mathf.Clamp(right, -1, 1);
        up = Mathf.Clamp(up, -1, 1);
        if (right == up && up == 0)
        {
            return false;
        }

        faceDir.x = right;
        faceDir.z = up;

        _performingAction = true;

        if (up == 1)
            _animator.SetInteger("Direction", 2);
        if (right == 1)
            _animator.SetInteger("Direction", 3);
        if (up == -1)
            _animator.SetInteger("Direction", 0);
        if (right == -1)
            _animator.SetInteger("Direction", 1);

        TileCoord newPosition = position + new TileCoord(right, up);
        TileCoord xChange = position + new TileCoord(right, 0);
        TileCoord yChange = position + new TileCoord(0, up);

        TileInfo tile = floor.tiles[floor.CoordToIndex(newPosition)];
        TileInfo xTile = floor.tiles[floor.CoordToIndex(xChange)];
        TileInfo yTile = floor.tiles[floor.CoordToIndex(yChange)];
        if (!tile.isWall && !xTile.isWall && !yTile.isWall && tile.occupyingEntity == null)
        {
            occupyingTile.occupyingEntity = null;
            occupyingTile = tile;
            occupyingTile.occupyingEntity = this;

            position = newPosition;
            Vector2 original = transform.position;
            Vector2 destined = tile.CoordToPosition();
            StartCoroutine(MoveCoroutine(original, destined));
            //_dungeonUI.AddEntry(gameObject.name + " moved!");
            _dgGameManager.TurnCompleted.Invoke(); // NOTE: POSSSIBLE TO TRIGGER MULTIPLE INTERACTIONS AT A TIME, MIGHT BUG OUT, MAKE IT ONLY WAIT IF IN VIEW
            return true;
        }
        _performingAction = false;
        return false;
    }

    public void Warp(TileCoord newPosition, bool animate = false) // No conditions for movement
    {
        _performingAction = true;
        if (occupyingTile != null)
        {
            occupyingTile.occupyingEntity = null;
        }
        position = newPosition;
        if (animate)
        {
            _performingAction = false;
        } else
        {
            TileInfo tile = floor.tiles[floor.CoordToIndex(position)];
            if (tile.occupyingEntity != null)
            {
                TileInfo shifted = floor.GetNextAvailable(tile.coord);
                tile.occupyingEntity.Warp(shifted.coord);
            }
            tile.occupyingEntity = this;
            occupyingTile = tile;
            transform.position = tile.CoordToPosition();
            _performingAction = false;
            if (_dgGameManager)
            {
                _dgGameManager.TurnCompleted.Invoke();
            }
        }
    }

    public List<TileCoord> Pathfind(TileCoord point) // Creates a list of cooords to follow
    {
        Floor.ClearSearch();
        List<TileCoord> movements = new List<TileCoord>();
        //AStarSearch(movements, Position, point);
        return movements;
    }

    protected void Update()
    {

    }

    protected new void Start()
    {
        base.Start();
        faceDir = new TileCoord(0, -1);
        _performingAction = false;
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _characterBehaviour = GetComponent<CharacterBehaviour>();
        _animator = GetComponent<Animator>();
        if (_characterBehaviour.character.associatedCharacter != CHARACTER_ENUM.NUM_CHARACTERS)
        {
            _animator.runtimeAnimatorController = CharacterProfiles.Instance.characterProfiles[(int)_characterBehaviour.character.associatedCharacter].animatorController;
        } else
        {
            _animator.runtimeAnimatorController = CharacterProfiles.Instance.characterProfiles[0].animatorController;
        }
    }
}
