#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.EventSystems.EventTrigger;

[CustomEditor(typeof(DGEntity))]
public class DGEntityEditor : Editor
{
    static int dirX;
    static int dirZ;
    static Vector2 heuristicStart, heuristicEnd;
    DGEntity myTarget;
    DGPlayer plr;
    private void OnEnable()
    {
        plr = FindAnyObjectByType<DGPlayer>();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as DGEntity;
        if (plr != null)
        {
            if (GUILayout.Button("Clear Markers"))
            {
                DebugTools.Instance.ClearMarkers();
            }
            if (GUILayout.Button("Get Closest"))
            {
                TileCoord pos = myTarget.GetClosestDirection(plr.Position);
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue);
            }
            if (GUILayout.Button("Test Pathfind"))
            {
                DebugTools.Instance.ClearMarkers();
                TileCoord pos = myTarget.GetClosestDirection(plr.Position);
                //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(myTarget.Position), Color.yellow);
                //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.magenta);
                //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue);
                List<TileCoord> path = myTarget.AStarPathfind(myTarget.Position, pos);
                //for (int i = 0; i < path.Count; i++)
                //{
                //    //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString());
                //    var pp = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(path[i])];
                //    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), pp.searchScore.ToString());
                //}
            }
            dirX = EditorGUILayout.IntField("Move X", Mathf.Clamp(dirX,-1,1));
            dirZ = EditorGUILayout.IntField("Move Z", Mathf.Clamp(dirZ, -1, 1));
            if (GUILayout.Button("Direction Test"))
            {
                TileCoord newPos = myTarget.Position + new TileCoord(dirX, dirZ);
                TileCoord currDiff = newPos - myTarget.Position;
                TileCoord xDiff = new TileCoord(myTarget.Position.x + currDiff.x, myTarget.Position.z);
                TileCoord zDiff = new TileCoord(myTarget.Position.x, myTarget.Position.z + currDiff.z);

                //TileInfo tile = myTarget.Floor.CoordToTileInfo(newPos);
                //TileInfo xTile = myTarget.Floor.CoordToTileInfo(xDiff);
                //TileInfo zTile = myTarget.Floor.CoordToTileInfo(zDiff);
                //tile.isWall || xTile.isWall || zTile.isWall

                DebugTools.Instance.ClearMarkers();
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(newPos));
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(xDiff), Color.blue);
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(zDiff), Color.yellow);
            }

            heuristicStart = EditorGUILayout.Vector2Field("Start", heuristicStart);
            heuristicEnd = EditorGUILayout.Vector2Field("End", heuristicEnd);
            if (GUILayout.Button("Distance Check"))
            {
                TileCoord start = new TileCoord((int)heuristicStart.x + myTarget.Position.x, (int)heuristicStart.y + myTarget.Position.z);
                TileCoord end = new TileCoord((int)heuristicEnd.x + myTarget.Position.x, (int)heuristicEnd.y + myTarget.Position.z);
                float dist = DGEntity.Heuristic(start, end);
                //TileInfo tile = myTarget.Floor.CoordToTileInfo(newPos);
                //TileInfo xTile = myTarget.Floor.CoordToTileInfo(xDiff);
                //TileInfo zTile = myTarget.Floor.CoordToTileInfo(zDiff);
                //tile.isWall || xTile.isWall || zTile.isWall

                DebugTools.Instance.ClearMarkers();
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(start), Color.blue, dist.ToString());
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(end), Color.yellow, dist.ToString());
            }

            if (GUILayout.Button("Perimeter Test"))
            {
                var directions = myTarget.Position.GetDirections();
                for (int i = directions.Count - 1; i >= 0; i--)
                {
                    if (!(DungeonFloor.IsInX(directions[i].x) && DungeonFloor.IsInZ(directions[i].z)))
                    {
                        directions.RemoveAt(i);
                        continue;
                    }
                }

                DebugTools.Instance.ClearMarkers();
                foreach (var pos in directions)
                {
                    var curr = myTarget.Position;
                    TileCoord currDiff = pos - curr;
                    TileCoord xDiff = new TileCoord(curr.x + currDiff.x, curr.z);
                    TileCoord zDiff = new TileCoord(curr.x, curr.z + currDiff.z);

                    TileInfo tile = myTarget.Floor.CoordToTileInfo(pos);
                    TileInfo xTile = myTarget.Floor.CoordToTileInfo(xDiff);
                    TileInfo zTile = myTarget.Floor.CoordToTileInfo(zDiff);
                    //tile.isWall || xTile.isWall || zTile.isWall
                    bool entityCheck = tile.occupyingEntity != null;
                    float dist = DGEntity.Heuristic(pos, myTarget.GetClosestDirection(plr.Position));
                    if (tile.isWall || xTile.isWall || zTile.isWall || entityCheck)
                    {
                        dist = -1;
                    }
                    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue, dist.ToString());
                }
            }
        }
    }
}
#endif