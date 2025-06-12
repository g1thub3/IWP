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
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as DGEntity;
        DGPlayer plr = FindAnyObjectByType<DGPlayer>();
        if (DebugTools.Instance.EntityDebugOn && plr != null)
        {
            if (GUILayout.Button("Test Pathfind"))
            {
                DebugTools.Instance.ClearMarkers();
                List<TileCoord> path = AStarPathfind(myTarget.Position, plr.Position);
                foreach (TileCoord coord in path)
                {
                    DebugTools.Instance.AddMarker(new Vector3(coord.x * TileInfo.tileScale, coord.z * TileInfo.tileScale, 0));
                }
            }
        }
    }

    private List<TileCoord> AStarPathfind(TileCoord start, TileCoord end)
    {
        myTarget.Floor.ClearSearch();
        List<TileCoord> path = new List<TileCoord>();

        path.Add(start);
        myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(start)].hasSearched = true;
        bool pathFound = false;
        while (!pathFound)
        {
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

                if (tile.isWall || xTile.isWall || zTile.isWall || tile.occupyingEntity != null || tilePP.hasSearched)
                {
                    tilePP.searchScore = -1;
                }
                else
                {
                    float dist = (tile.coord.DistanceSquared(end) - curr.DistanceSquared(end)) * -1;
                    if (dist > 0)
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
                if (found == end)
                    pathFound = true;
            } else
            {
                path.Remove(path.Last());
            }
        }

        return path;
    }
}
