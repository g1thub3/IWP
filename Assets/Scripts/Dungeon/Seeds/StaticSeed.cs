using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

[System.Serializable]
public class StaticNPCGenData
{
    public CHARACTER_ENUM character;
    public int level;
    public int x, y;
}

[System.Serializable]
public class StaticItemGenData
{
    public ItemData item;
    public int x, y;
}

[CreateAssetMenu(fileName = "StaticSeed", menuName = "Dungeon Seeds/Static Seed")]
public class StaticSeed : DGSeed
{
    public int playerSpawnX, playerSpawnY;
    public List<StaticNPCGenData> enemyPlacements;
    public List<StaticItemGenData> itemPlacements;

    [HideInInspector] public bool[] tiles;

    private void OnEnable()
    {
        if (tiles == null)
        {
            tiles = new bool[DungeonFloor.floorSize * DungeonFloor.floorSize];
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = true;
            }
        }
    }

    public override DungeonFloor Generate(DGData dungeonData)
    {
        DebugTools.Instance.ClearMarkers();
        DungeonFloor floorData = new DungeonFloor();
        floorData.Fill();
        floorData.nonWallTiles = new List<TileInfo>();
        for (int i = 0; i < tiles.Length; i++)
        {
            floorData.tiles[i].isWall = tiles[i];
            if (!floorData.tiles[i].isWall)
            {
                floorData.nonWallTiles.Add(floorData.tiles[i]);
            }
        }

        List<FloorRoom> rooms = new List<FloorRoom>();

        var newRoom = new FloorRoom();
        newRoom.origin = new TileCoord(0, 0);
        newRoom.length = DungeonFloor.floorSize;
        newRoom.height = DungeonFloor.floorSize;
        rooms.Add(newRoom);
        floorData.rooms = rooms;

        return floorData;
    }

    public override void AddEnemies(DGGenerator dgGen)
    {
        if (enemyPlacements == null) return;
        foreach (var enemy in enemyPlacements) {
            TileCoord point = new TileCoord(enemy.x, enemy.y);
            CharacterEntry newCharacter = CharacterEntry.Create(enemy.character, enemy.level);
            dgGen.SpawnNPC(DG_CHARACTER_TYPE.ENEMY, newCharacter, point);
        }
    }

    public override void AddItems(DGGenerator dungeonGen)
    {
        if (itemPlacements == null) return;
        foreach (var item in itemPlacements) {
            TileCoord point = new TileCoord(item.x, item.y);
            dungeonGen.InsertItem(Tilesets.Instance.ConstructItemInteractable(Item.New(item.item)), dungeonGen.CurrentFloor.CoordToTileInfo(point));
        }
    }
}