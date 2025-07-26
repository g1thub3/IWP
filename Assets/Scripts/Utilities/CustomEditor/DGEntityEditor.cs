#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(DGEntity))]
public class DGEntityEditor : Editor
{
    static int dirX;
    static int dirZ;
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
        if (DebugTools.Instance.EntityDebugOn && plr != null)
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
                //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(pos), Color.blue);
                List<TileCoord> path = myTarget.AStarPathfind(myTarget.Position, pos);
                //for (int i = 0; i < path.Count; i++)
                //{
                //    //DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString());
                //    var pp = myTarget.Floor.tilePathPoints[myTarget.Floor.CoordToIndex(path[i])];
                //    DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), pp.searchScore.ToString());
                //}
            }
            dirX = EditorGUILayout.IntField("Move X", dirX);
            dirZ = EditorGUILayout.IntField("Move X", dirZ);
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
        }
    }
}
#endif