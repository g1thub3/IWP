using System.Collections.Generic;
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
                myTarget.Floor.ClearSearch();
                List<TileCoord> path = new List<TileCoord>();
                AStarDebug(path, myTarget.Position, plr.Position);
                foreach (TileCoord coord in path)
                {
                    DebugTools.Instance.AddMarker(new Vector3(coord.x * TileInfo.tileScale, coord.z * TileInfo.tileScale, 0));
                }
            }
        }
    }

    // LISTS CAN BE FILLED WITH FUNCTIONS


    private int AStarDebug(List<TileCoord> toFill, TileCoord curr, TileCoord end)
    {
        myTarget.Floor.CoordToTileInfo(curr).hasBeenSearched = true;
        if (curr.Equals(end))
        {
            toFill.Add(curr);
            Debug.Log("Found!");
            return 0;
        }

        List<TileInfo> directions = new List<TileInfo>();
        TileInfo N = myTarget.Floor.CoordToTileInfo(curr.North);
        TileInfo NE = myTarget.Floor.CoordToTileInfo(curr.Northeast);
        TileInfo E = myTarget.Floor.CoordToTileInfo(curr.East);
        TileInfo SE = myTarget.Floor.CoordToTileInfo(curr.Southeast);
        TileInfo S = myTarget.Floor.CoordToTileInfo(curr.South);
        TileInfo SW = myTarget.Floor.CoordToTileInfo(curr.Southwest);
        TileInfo W = myTarget.Floor.CoordToTileInfo(curr.West);
        TileInfo NW = myTarget.Floor.CoordToTileInfo(curr.Northwest);
        if (N != null)
            directions.Add(N);
        if (NE != null)
            directions.Add(NE);
        if (E != null)
            directions.Add(E);
        if (SE != null)
            directions.Add(SE);
        if (S != null)
            directions.Add(S);
        if (SW != null)
            directions.Add(SW);
        if (W != null)
            directions.Add(W);
        if (NW != null)
            directions.Add(NW);

        foreach (var dir in directions)
        {
            TileCoord currDiff = dir.coord - curr;
            TileCoord xDiff = new TileCoord(curr.x + currDiff.x, curr.z);
            TileCoord zDiff = new TileCoord(curr.x, curr.z + currDiff.z);

            TileInfo xTile = myTarget.Floor.CoordToTileInfo(xDiff);
            TileInfo yTile = myTarget.Floor.CoordToTileInfo(zDiff);
            if (dir.isWall || dir.occupyingEntity != null || dir.hasBeenSearched)
            {
                dir.searchScore = -1;
            }
            else
            {
                float dist = dir.coord.DistanceSquared(end) - curr.DistanceSquared(end) * -1;
                if (dist > 0)
                {
                    dist *= dist;
                }
                dist *= dist;
                dir.searchScore = 1 + dist;
            }
        }

        //foreach (var coord in directions)
        //{
        //    DebugTools.Instance.AddMarker(coord.CoordToPosition(), coord.searchScore.ToString());
        //}
        for (int i = directions.Count - 1; i >= 0; i--)
        {
            if (directions[i].searchScore == -1)
                directions.RemoveAt(i);
        }

        for (int i = 0; i < directions.Count - 1; i++)
        {
            for (int j = i; j < directions.Count - 1; j++)
            {
                if (directions[j].searchScore > directions[i].searchScore)
                {
                    var temp = directions[i];
                    directions[i] = directions[j];
                    directions[j] = temp;
                }
            }
        }

        for (int i = 0; i < directions.Count - 1; i++)
        {
            int search = AStarDebug(toFill, directions[i].coord, end);
            if (search == 0)
            {
                toFill.Add(curr);
                return 0;
            }
        }
        return -1;
    }
}
