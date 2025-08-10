using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Search;
using UnityEngine;

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

public enum ANIMATION_ENUM { 
    IDLE,
    WALK,
    JAB,
    CAST,
    SLASH
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
    protected FloorRoom _currRoom;
    protected DGGenerator _dungeonGen;
    private string[] _actionKeys;
    private string _direction;
    public ANIMATION_ENUM _action;

    protected TileCoord faceDir;
    private Transition _moveTransition;

    public TileCoord FaceDir
    {
        get {
            if (faceDir == null)
                faceDir = new TileCoord(0, -1);
            return faceDir; 
        }
        set
        {
            if (faceDir == null)
                faceDir = new TileCoord(0, -1);
            faceDir.x = value.x;
            faceDir.z = value.z;
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

    public bool IsPerformingAction
    {
        get { return _performingAction; }
        set { _performingAction = value; }
    }
    public FloorRoom CurrentRoom { get { return _currRoom; } }
    
    private IEnumerator InteractableYield(DGInteractable interacted)
    {
        _performingAction = true;
        interacted.OnInteract(this, floor);
        while (interacted.interaction.IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        _performingAction = false;
    }

    public IEnumerator MoveCoroutine(Vector2 original, Vector2 destined, bool isSwap = false)
    {
        _action = ANIMATION_ENUM.WALK;
        float t = 0;
        while (t < _moveTime)
        {
            t += Time.deltaTime;
            transform.position = Vector2.Lerp(original, destined, t / _moveTime);
            yield return new WaitForEndOfFrame();
        }
        transform.position = destined;
        _action = ANIMATION_ENUM.IDLE;

        TileInfo tile = floor.tiles[floor.CoordToIndex(position)];
        if (tile.item != null)
        {
            tile.item.OnInteract(this, floor);
            while (tile.item.interaction.IsInProgress())
            {
                yield return new WaitForEndOfFrame();
            }
        }
        if (tile.structure != null)
        {
            tile.structure.OnInteract(this, floor);
            while (tile.structure.interaction.IsInProgress())
            {
                yield return new WaitForEndOfFrame();
            }
        }
        if (!isSwap)
            _performingAction = false;
        _dungeonUI.UpdateMinimap();
    }

    public bool InteractAction()
    {
        TileInfo tile = floor.tiles[floor.CoordToIndex(position + faceDir)];
        if (tile.item != null)
        {
            if (tile.item.CanInteractWithAction)
            {
                StartCoroutine(InteractableYield(tile.item));
                return true;
            }
        }
        if (tile.structure != null)
        {
            if (tile.structure.CanInteractWithAction)
            {
                StartCoroutine(InteractableYield(tile.structure));
                return true;
            }
        }
        if (tile.occupyingEntity != null)
        {
            if (tile.occupyingEntity.TryGetComponent<DGInteractable>(out DGInteractable interactable))
            {
                if (interactable.CanInteractWithAction)
                {
                    StartCoroutine(InteractableYield(interactable));
                    return true;
                }
            }
        }
        return false;
    }

    public void RefreshRoom()
    {
        _currRoom = null;
        foreach (var room in _dungeonGen.CurrentFloor.rooms)
        {
            if (room.IsCoordInRoom(Position))
            {
                _currRoom = room;
                break;
            }
        }
    }

    public bool Move(int right, int up) // Limited movement
    {
        right = Mathf.Clamp(right, -1, 1);
        up = Mathf.Clamp(up, -1, 1);
        if (right == up && up == 0)
        {
            return false;
        }

        FaceDirection(right, up);

        TileCoord newPosition = position + new TileCoord(right, up);
        TileCoord xChange = position + new TileCoord(right, 0);
        TileCoord yChange = position + new TileCoord(0, up);

        _performingAction = true;

        TileInfo tile = floor.tiles[floor.CoordToIndex(newPosition)];
        TileInfo xTile = floor.tiles[floor.CoordToIndex(xChange)];
        TileInfo yTile = floor.tiles[floor.CoordToIndex(yChange)];
        if (!tile.isWall && !xTile.isWall && !yTile.isWall)
        {
            if (tile.occupyingEntity == null)
            {
                occupyingTile.occupyingEntity = null;
                occupyingTile = tile;
                occupyingTile.occupyingEntity = this;

                position = newPosition;

                RefreshRoom();

                Vector2 original = transform.position;
                Vector2 destined = tile.CoordToPosition();
                _dgGameManager.TurnCompleted.Invoke();

                StartCoroutine(MoveCoroutine(original, destined));
                return true;
            } else if (tile.occupyingEntity.GetComponent<CharacterBehaviour>().alliance == 0 && _characterBehaviour.alliance == 0) // Swap position with allies
            {
                var temp = tile.occupyingEntity;
                var oldTile = occupyingTile;

                occupyingTile = tile;
                occupyingTile.occupyingEntity = this;

                oldTile.occupyingEntity = temp;
                temp.occupyingTile = oldTile;

                temp.position = position;
                position = newPosition;

                RefreshRoom();
                temp.RefreshRoom();

                Vector2 original = transform.position;
                Vector2 destined = tile.CoordToPosition();
                _dgGameManager.TurnCompleted.Invoke();

                if (-up == 1)
                    temp.NumToDir(2);
                if (-right == 1)
                    temp.NumToDir(3);
                if (-up == -1)
                    temp.NumToDir(0);
                if (-right == -1)
                    temp.NumToDir(1);

                StartCoroutine(MoveCoroutine(original, destined));
                StartCoroutine(temp.MoveCoroutine(destined, original, true));
                return true;
            }

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
    public void Wait()
    {
        if (_dgGameManager.CurrentEntityTurn() == this)
            _dgGameManager.TurnCompleted.Invoke();
    }

    public TileCoord GetClosestDirection(TileCoord pt, bool ignoreEntity = false)
    {
        float closestDist = -1;
        TileCoord closest = null;
        var directions = pt.GetDirections();
        for (int i = directions.Count - 1; i >= 0; i--)
        {
            if (!(DungeonFloor.IsInX(directions[i].x) && DungeonFloor.IsInZ(directions[i].z)))
            {
                directions.Remove(directions[i]);
                continue;
            }

            var tile = Floor.CoordToTileInfo(directions[i]);

            TileCoord currDiff = directions[i] - pt;
            TileCoord xDiff = new TileCoord(pt.x + currDiff.x, pt.z);
            TileCoord zDiff = new TileCoord(pt.x, pt.z + currDiff.z);

            TileInfo xTile = Floor.CoordToTileInfo(xDiff);
            TileInfo zTile = Floor.CoordToTileInfo(zDiff);
            if (directions[i].Equals(Position)) {
                if (!(xTile.isWall || zTile.isWall))
                {
                    return directions[i];
                }
            };
            if (tile.isWall || xTile.isWall || zTile.isWall || (tile.occupyingEntity != null && ignoreEntity == false))
            {
                directions.Remove(directions[i]);
                continue;
            }
        }
        for (int i = 0; i < directions.Count; i++)
        {
            float dist = Position.DistanceSquared(directions[i]);
            if (closestDist == -1 || dist < closestDist)
            {
                closest = directions[i];
                closestDist = dist;
            }
        }
        if (closest == null)
            return pt;
        return closest;
    }


    //public bool AllianceScan(DGEntity toSearch)
    //{
    //    bool isConnected = false;
    //    List<TileCoord> searched = new List<TileCoord>();
    //    TileCoord curr = Position;
    //    while (!isConnected)
    //    {
    //        bool allianceFound = false;
    //        var directions = curr.GetDirections();
    //        foreach (var direction in directions) {
    //            if (!(DungeonFloor.IsInZ(direction.z) && DungeonFloor.IsInX(direction.x))) continue;
    //            bool alrSearched = false;
    //            foreach (var coord in searched)
    //            {
    //                if (coord.Equals(direction))
    //                {
    //                    alrSearched = true;
    //                    break;
    //                }
    //            }
    //            if (alrSearched) continue;
    //            var tile = Floor.CoordToTileInfo(direction);
    //            var entity = tile.occupyingEntity;
    //            if (entity != null)
    //            {
    //                if (entity == toSearch)
    //                {
    //                    return true;
    //                } else if (entity.GetComponent<CharacterBehaviour>().alliance == _characterBehaviour.alliance)
    //                {
    //                    searched.Add(curr);
    //                    curr = direction;
    //                    allianceFound = true;
    //                }
    //            }
    //        }
    //        if (!allianceFound)
    //            break;
    //    }
    //    return false;
    //}

    public static float Heuristic(TileCoord curr, TileCoord end)
    {
        float D = 1;
        float D2 = Mathf.Sqrt(2);
        float dx = Mathf.Abs(curr.x - end.x);
        float dz = Mathf.Abs(curr.z - end.z);
        float h = D * (dx + dz) + (D2 - 2 * D) * Mathf.Min(dx, dz);
        return h;
    }

    public List<TileCoord> AStarPathfind(TileCoord start, TileCoord end)
    {
        Floor.ClearSearch();
        List<TileCoord> path = new List<TileCoord>();
        List<float> scores = new List<float>();
        path.Add(start);

        Floor.tilePathPoints[Floor.CoordToIndex(start)].hasSearched = true;
        scores.Add(0);
        if (start.Equals(end))
            return path;
        bool pathFound = false;
        //int ID = 0;
        while (!pathFound)
        {
            if (path.Count == 0)
            {
                Debug.Log("NPC Pathfind failed.");
                break;
            }
            TileCoord curr = path.Last<TileCoord>();
            List<TileInfo> searchableTiles = new List<TileInfo>();
            var directions = curr.GetDirections();
            for (int i = directions.Count - 1; i >= 0; i--)
            {
                if (!(DungeonFloor.IsInX(directions[i].x) && DungeonFloor.IsInZ(directions[i].z)))
                {
                    directions.RemoveAt(i);
                    continue;
                }
                TileInfo tile = Floor.CoordToTileInfo(directions[i]);
                searchableTiles.Add(tile);
            }
            foreach (var tile in searchableTiles)
            {
                //ID++;
                var tilePP = Floor.tilePathPoints[Floor.CoordToIndex(tile.coord)];
                TileCoord currDiff = tile.coord - curr;
                int ogX, ogZ;
                ogX = curr.x;
                ogZ = curr.z;
                TileCoord xDiff = new TileCoord(ogX + currDiff.x, ogZ);
                TileCoord zDiff = new TileCoord(ogX, ogZ + currDiff.z);

                TileInfo xTile = Floor.CoordToTileInfo(xDiff);
                TileInfo zTile = Floor.CoordToTileInfo(zDiff);
                //tile.isWall || xTile.isWall || zTile.isWall
                bool entityCheck = tile.occupyingEntity != null;
                if (tile.isWall || xTile.isWall || zTile.isWall || entityCheck || tilePP.hasSearched)
                {
                    //if (DebugTools.Instance.EntityDebugOn && zTile.isWall) {
                    //    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(curr), Color.magenta, ID.ToString());
                    //    DebugTools.Instance.AddMarker(zTile.CoordToPosition(), Color.yellow, ID.ToString());
                    //}
                    if (DebugTools.Instance.EntityDebugOn && zTile.isWall) {
                        DebugTools.Instance.AddMarker(tile.CoordToPosition(),
                            string.Format("Wall={0}\nXWall={1}\nZWall={2}\nOccupied={3}\nSearched={4}", tile.isWall, xTile.isWall, zTile.isWall, entityCheck, tilePP.hasSearched));
                        DebugTools.Instance.AddMarker(zTile.CoordToPosition(), Color.magenta);
                    }
                    tilePP.searchScore = -1;
                }
                else
                {
                    tilePP.searchScore = Heuristic(tile.coord, end);
                }
            }

            for (int i = 0; i < searchableTiles.Count - 1; i++)
            {
                for (int j = i; j < searchableTiles.Count - 1; j++)
                {
                    var iPP = Floor.tilePathPoints[Floor.CoordToIndex(searchableTiles[i].coord)];
                    var jPP = Floor.tilePathPoints[Floor.CoordToIndex(searchableTiles[j].coord)];
                    if (jPP.searchScore < iPP.searchScore)
                    {
                        var temp = searchableTiles[i];
                        searchableTiles[i] = searchableTiles[j];
                        searchableTiles[j] = temp;
                    }
                }
            }

            TileCoord found = null;
            for (int i = 0; i < searchableTiles.Count; i++)
            {
                var tilePP = Floor.tilePathPoints[Floor.CoordToIndex(searchableTiles[i].coord)];
                if (tilePP.searchScore != -1)
                {
                    found = searchableTiles[i].coord;
                    break;
                }
            }
            //if (path.Count == 1)
            //{
            //    DebugTools.Instance.ClearMarkers();
            //    for (int i = 0; i < searchableTiles.Count; i++)
            //    {
            //        var tilePP = Floor.tilePathPoints[Floor.CoordToIndex(searchableTiles[i].coord)];
            //        DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(searchableTiles[i].coord), tilePP.searchScore.ToString());
            //    }
            //}
            if (found != null)
            {
                path.Add(found);
                var tilePP = Floor.tilePathPoints[Floor.CoordToIndex(found)];
                scores.Add(tilePP.searchScore);
                Floor.tilePathPoints[Floor.CoordToIndex(found)].hasSearched = true;
                if (found.Equals(end))
                    pathFound = true;
            }
            else
            {
                path.Remove(path.Last());
                scores.Remove(scores.Last());
            }
        }
        //DebugTools.Instance.ClearMarkers();
        //for (int i = 0; i < path.Count; i++)
        //{
        //    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString() + " (" + (scores[i].ToString()) + ")");
        //}

        return path;
    }

    protected void Update()
    {
        PlayAnimation();
    }

    protected void NumToDir(int num)
    {
        switch (num)
        {
            case 1:
                _direction = "west";
                break;
            case 2:
                _direction = "north";
                break;
            case 3:
                _direction = "east";
                break;
            default:
            case 0:
                _direction = "south";
                break;
        }
    }

    private void PlayAnimation()
    {
        string state = string.Format("{0}_{1}", _actionKeys[(int)_action], _direction != null ? _direction : "south");
        _animator.Play(state);
    }

    public void FaceDirection(int x, int z)
    {
        if (x == z && z == 0)
            return;
        if (faceDir == null)
            faceDir = new TileCoord(0, -1);

        faceDir.x = x;
        faceDir.z = z;

        if (faceDir.z == 1)
            NumToDir(2);
        if (faceDir.x == 1)
            NumToDir(3);
        if (faceDir.z == -1)
            NumToDir(0);
        if (faceDir.x == -1)
            NumToDir(1);
    }
    protected new void Start()
    {
        base.Start();
        _moveTransition = new Transition();
        _moveTransition.max = _moveTime;
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
        _performingAction = false;
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        _characterBehaviour = GetComponent<CharacterBehaviour>();
        _animator = GetComponent<Animator>();

        if (_characterBehaviour.character.associatedCharacter != CHARACTER_ENUM.NUM_CHARACTERS)
        {
            _animator.runtimeAnimatorController = CharacterProfiles.Instance.characterProfiles[(int)_characterBehaviour.character.associatedCharacter].animatorController;
        }
        else
        {
            _animator.runtimeAnimatorController = CharacterProfiles.Instance.characterProfiles[0].animatorController;
        }
        _action = ANIMATION_ENUM.IDLE;
        _actionKeys = new string[5];
        _actionKeys[(int)ANIMATION_ENUM.IDLE] = "idle";
        _actionKeys[(int)ANIMATION_ENUM.WALK] = "walk";
        _actionKeys[(int)ANIMATION_ENUM.JAB] = "jab";
        _actionKeys[(int)ANIMATION_ENUM.CAST] = "cast";
        _actionKeys[(int)ANIMATION_ENUM.SLASH] = "slash";
    }
}
