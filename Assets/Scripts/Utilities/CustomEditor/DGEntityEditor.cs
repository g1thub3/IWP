using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(DGEntity))]
public class DGEntityEditor : Editor
{
    DGEntity myTarget;
    int iterationCounter;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as DGEntity;
        DGPlayer plr = FindAnyObjectByType<DGPlayer>();
        if (DebugTools.Instance.EntityDebugOn && plr != null)
        {
            GUILayout.Button(iterationCounter.ToString());
            if (GUILayout.Button("Test Pathfind"))
            {
                DebugTools.Instance.ClearMarkers();
                TileCoord pos = GetClosestDirection(plr.Position);
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue);
                List<TileCoord> path = AStarPathfind(myTarget.Position, pos);
                for (int i = 0; i < path.Count; i++)
                {
                    //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString());
                    var pp = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(path[i])];
                    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), pp.searchScore.ToString());
                }
            }
            if (GUILayout.Button("Next Iteration"))
            {
                iterationCounter++;
                DebugTools.Instance.ClearMarkers();
                TileCoord pos = GetClosestDirection(plr.Position);
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue);
                List<TileCoord> path = AStarPathfind(myTarget.Position, pos);
                for (int i = 0; i < path.Count; i++)
                {
                    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString());
                }
            }
            if (GUILayout.Button("Prev Iteration"))
            {
                iterationCounter--;
                DebugTools.Instance.ClearMarkers();
                TileCoord pos = GetClosestDirection(plr.Position);
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue);
                List<TileCoord> path = AStarPathfind(myTarget.Position, pos);
                for (int i = 0; i < path.Count; i++)
                {
                    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString());
                }
            }
        }
    }

    private TileCoord GetClosestDirection(TileCoord pt)
    {
        float closestDist = -1;
        TileCoord closest = null;
        var directions = pt.GetDirections();
        for (int i = directions.Count - 1; i >=0; i--)
        {
            if (!(DungeonFloor.IsInX(directions[i].x) && DungeonFloor.IsInZ(directions[i].z)))
            {
                directions.Remove(directions[i]);
                continue;
            }

            var tile = myTarget.Floor.CoordToTileInfo(directions[i]);

            TileCoord currDiff = directions[i] - pt;
            TileCoord xDiff = new TileCoord(pt.x + currDiff.x, pt.z);
            TileCoord zDiff = new TileCoord(pt.x, pt.z + currDiff.z);

            TileInfo xTile = myTarget.Floor.CoordToTileInfo(xDiff);
            TileInfo zTile = myTarget.Floor.CoordToTileInfo(zDiff);
            if (tile.isWall || xTile.isWall || zTile.isWall || tile.occupyingEntity != null)
            {
                directions.Remove(directions[i]);
            }
        }
        for (int i = 0; i <directions.Count; i++)
        {
            float dist = myTarget.Position.DistanceSquared(directions[i]);
            if (closestDist == -1 || dist < closestDist)
            {
                closest = directions[i];
                closestDist = dist;
            }
        }
        return closest;
    }

    private List<TileCoord> AStarPathfind(TileCoord start, TileCoord end)
    {
        int itCounter = 0;
        myTarget.Floor.ClearSearch();
        List<TileCoord> path = new List<TileCoord>();
        path.Add(start);
        myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(start)].hasSearched = true;
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
            for (int i = directions.Count - 1; i >=0; i--)
            {
                if (!(DungeonFloor.IsInX(directions[i].x) && DungeonFloor.IsInZ(directions[i].z)))
                {
                    directions.RemoveAt(i);
                    continue;
                }
                TileInfo tile = myTarget.Floor.CoordToTileInfo(directions[i]);
                searchableTiles.Add(tile);
            }
            foreach (var tile in searchableTiles)
            {
                var tilePP = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(tile.coord)];
                TileCoord currDiff = tile.coord - curr;
                TileCoord xDiff = new TileCoord(curr.x + currDiff.x, curr.z);
                TileCoord zDiff = new TileCoord(curr.x, curr.z + currDiff.z);

                TileInfo xTile = myTarget.Floor.CoordToTileInfo(xDiff);
                TileInfo zTile = myTarget.Floor.CoordToTileInfo(zDiff);
                //tile.isWall || xTile.isWall || zTile.isWall
                if (tile.isWall || xTile.isWall || zTile.isWall || tile.occupyingEntity != null || tilePP.hasSearched)
                {
                    tilePP.searchScore = -1;
                }
                else
                {
                    float currToEnd = curr.DistanceSquared(end);
                    float newToEnd = tile.coord.DistanceSquared(end);
                    float dist = newToEnd - currToEnd;
                    if (newToEnd < currToEnd)
                    {
                        dist *= dist;
                    }
                    dist *= dist;
                    tilePP.searchScore = 1 + dist;
                }
            }

            for (int i = 0; i < searchableTiles.Count - 1; i++)
            {
                for (int j = i; j < searchableTiles.Count - 1; j++)
                {
                    var iPP = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(searchableTiles[i].coord)];
                    var jPP = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(searchableTiles[j].coord)];
                    if (jPP.searchScore > iPP.searchScore)
                    {
                        var temp = searchableTiles[i];
                        searchableTiles[i] = searchableTiles[j];
                        searchableTiles[j] = temp;
                    }
                }
            }

            TileCoord found = null;
            if (searchableTiles.Count > 0)
            {
                var tilePP = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(searchableTiles[0].coord)];
                if (tilePP.searchScore != -1)
                {
                    found = searchableTiles[0].coord;
                }
            }
            if (found != null)
            {
                path.Add(found);
                myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(found)].hasSearched = true;
                //if (itCounter == iterationCounter)
                //    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(found));
                if (found.Equals(end))
                    pathFound = true;
            } else
            {
                path.Remove(path.Last());
                itCounter++;
            }
        }

        return path;
    }
}
