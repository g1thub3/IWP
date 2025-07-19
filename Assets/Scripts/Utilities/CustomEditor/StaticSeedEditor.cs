#if UNITY_EDITOR
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEngine;
using static CutsceneEditor;
using static CutsceneFunctions;

[CustomEditor(typeof(StaticSeed))]
public class StaticSeedEditor : Editor
{
    Texture black, white, red, teal, yellow;
    private readonly int gridSize = 10;

    private int ScaleWithGrid(int amt)
    {
        return amt * gridSize;
    }

    private void OnEnable()
    {
        if (black == null || white == null)
        {
            Texture[] editorTextures = Resources.LoadAll<Texture>("EditorTexture");
            if (editorTextures != null)
            {
                foreach (Texture editorTexture in editorTextures) {
                    if (editorTexture.name == "blackbox")
                        black = editorTexture;
                    else if (editorTexture.name == "whitebox")
                        white = editorTexture;
                    else if (editorTexture.name == "redbox")
                        red = editorTexture;
                    else if (editorTexture.name == "tealbox")
                        teal = editorTexture;
                    else if (editorTexture.name == "yellowbox")
                        yellow = editorTexture;
                }
            }

        }
    }

    StaticSeed myTarget;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        myTarget = target as StaticSeed;

        EditorGUILayout.LabelField("Dungeon Grid");
        if (black == null || white == null) {

            EditorGUILayout.LabelField("Textures failed to load");
            return;
        }

        //Rect r = EditorGUILayout.BeginVertical();
        GUIStyle gridStyle = new GUIStyle(GUI.skin.box);
        gridStyle.padding = new RectOffset(1, 1, 1, 1);
        gridStyle.margin = new RectOffset(0, 0, 0, 0);

        GUIStyle iconStyle = new GUIStyle(GUI.skin.box);
        //iconStyle.padding = new RectOffset(5, 5, 5, 5);
        Rect origin = EditorGUILayout.BeginVertical();
        for (int i = DungeonFloor.floorSize - 1; i >= 0; i--)
        {
            Rect rX = EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < DungeonFloor.floorSize; j++)
            {
                Rect dimensions = new Rect(rX.x + (gridSize * j), rX.y + (gridSize * i), gridSize, gridSize);
                int index = (i * DungeonFloor.floorSize) + j;
                if (myTarget.tiles[index] == true)
                {
                    if (GUILayout.Button(black, gridStyle, GUILayout.Width(gridSize), GUILayout.Height(gridSize)))
                    {
                        myTarget.tiles[index] = false;
                    }
                } else
                {
                    if (GUILayout.Button(white, gridStyle, GUILayout.Width(gridSize), GUILayout.Height(gridSize)))
                    {
                        myTarget.tiles[index] = true;
                    }
                }
                //if (i == myTarget.playerSpawnY && j == myTarget.playerSpawnX)
                //{
                //    playerToRender = dimensions;
                //} else
                //{
                //    bool enemyFound = false;
                //    foreach (var enemy in myTarget.enemyPlacements)
                //    {
                //        if (i == enemy.y && j == enemy.x)
                //        {
                //            enemiesToRender.Add(dimensions);
                //            enemyFound = true;
                //            break;
                //        }
                //    }
                //    if (!enemyFound)
                //    {
                //        foreach (var item in myTarget.itemPlacements)
                //        {
                //            if (i == item.y && j == item.x)
                //            {
                //                itemsToRender.Add(dimensions);
                //                break;
                //            }
                //        }
                //    }
                //}
            }
            EditorGUILayout.EndHorizontal();
        }

        GUI.Box(new Rect(origin.x + ScaleWithGrid(myTarget.playerSpawnX), 
            origin.y + ScaleWithGrid(DungeonFloor.floorSize - 1) - ScaleWithGrid(myTarget.playerSpawnY), gridSize, gridSize), yellow, iconStyle);
        EditorGUILayout.EndVertical();
        foreach (var enemy in myTarget.enemyPlacements)
        {
            GUI.Box(new Rect(origin.x + ScaleWithGrid(enemy.x), 
                origin.y + ScaleWithGrid(DungeonFloor.floorSize - 1) - ScaleWithGrid(enemy.y), 
                gridSize, gridSize), red, iconStyle);
        }
        foreach (var item in myTarget.itemPlacements)
        {
            GUI.Box(new Rect(origin.x + ScaleWithGrid(item.x), 
                origin.y + ScaleWithGrid(DungeonFloor.floorSize - 1) - ScaleWithGrid(item.y), 
                gridSize, gridSize), teal, iconStyle);
        }

        //EditorGUILayout.EndVertical();
    }
}
#endif