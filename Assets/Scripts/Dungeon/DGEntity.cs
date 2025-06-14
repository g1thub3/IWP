using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public TileCoord faceDir;

    public bool IsPerformingAction
    {
        get { return _performingAction; }
        set { _performingAction = value; }
    }
    public FloorRoom CurrentRoom { get { return _currRoom; } }
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
        _dungeonUI.UpdateMinimap();
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

            _currRoom = null;
            foreach (var room in _dungeonGen.CurrentFloor.rooms)
            {
                if (room.IsCoordInRoom(Position))
                {
                    _currRoom = room;
                    break;
                }
            }

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

    public TileCoord GetClosestDirection(TileCoord pt)
    {
        float closestDist = -1;
        TileCoord closest = null;
        var directions = pt.GetDirections();
        for (int i = directions.Count - 1; i >= 0; i--)
        {
            if (directions[i].Equals(Position)) return directions[i];
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
            if (tile.isWall || xTile.isWall || zTile.isWall || tile.occupyingEntity != null)
            {
                directions.Remove(directions[i]);
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


    private float Heuristic(TileCoord curr, TileCoord end)
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
                var tilePP = Floor.tilePathPoints[Floor.CoordToIndex(tile.coord)];
                TileCoord currDiff = tile.coord - curr;
                TileCoord xDiff = new TileCoord(curr.x + currDiff.x, curr.z);
                TileCoord zDiff = new TileCoord(curr.x, curr.z + currDiff.z);

                TileInfo xTile = Floor.CoordToTileInfo(xDiff);
                TileInfo zTile = Floor.CoordToTileInfo(zDiff);
                //tile.isWall || xTile.isWall || zTile.isWall
                if (tile.isWall || xTile.isWall || zTile.isWall || tile.occupyingEntity != null || tilePP.hasSearched)
                {
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

    }

    protected new void Start()
    {
        base.Start();
        faceDir = new TileCoord(0, -1);
        _dungeonGen = FindAnyObjectByType<DGGenerator>();
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
