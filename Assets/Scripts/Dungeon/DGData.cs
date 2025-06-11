using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class TileCoord // To be serialized, dont need to store these as floats since they're multiplied by tileScale anyway
{
    public int x;
    public int z;
    public TileCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(TileCoord other)
    {
        if (other == null) return false;
        return (x == other.x && z == other.z);
    }
    public bool Equals(int oX, int oZ)
    {
        return (x == oX && z == oZ);
    }
    public override string ToString()
    {
        return "(" + x + ", " + z + ")";
    }

    public float DistanceSquared(TileCoord other)
    {
        TileCoord final = this - other;
        return (final.z * final.z) + (final.x * final.x);
    }

    public TileCoord North
    {
        get { return new TileCoord(x, z + 1); }
    }
    public TileCoord Northeast
    {
        get { return new TileCoord(x + 1, z + 1); }
    }
    public TileCoord East
    {
        get { return new TileCoord(x + 1, z); }
    }
    public TileCoord Southeast
    {
        get { return new TileCoord(x + 1, z - 1); }
    }
    public TileCoord South
    {
        get { return new TileCoord(x, z - 1); }
    }
    public TileCoord Southwest
    {
        get { return new TileCoord(x - 1, z - 1); }
    }
    public TileCoord West
    {
        get { return new TileCoord(x - 1, z); }
    }
    public TileCoord Northwest
    {
        get { return new TileCoord(x - 1, z + 1); }
    }

    public static TileCoord operator +(TileCoord a, TileCoord b)
    {
        return new TileCoord(a.x + b.x, a.z + b.z);
    }
    public static TileCoord operator -(TileCoord a, TileCoord b)
    {
        return new TileCoord(a.x - b.x, a.z - b.z);
    }
}

[System.Serializable]
public class TileInfo // Use CastEnum after doing FromJson
{
    // contain the entity, structure and items here

    // STATIC
    public static readonly float tileScale = 2f;

    // PROPERTIES
    public bool isWall;

    public TileCoord coord;
    public Vector3 CoordToPosition()
    {
        return new Vector3(coord.x * tileScale, coord.z * tileScale, 0);
    }
    public TileInfo(TileCoord newCoord)
    {
        coord = newCoord;
    }
    public override string ToString()
    {
        return "Is Wall: " + isWall + " | Coord: " + coord.ToString();
    }

    public DGInteractable structure;
    public DGInteractable item;
    public DGEntity occupyingEntity;
    public void AddStructure(DGInteractable newStructure)
    {
        var newObj = Transform.Instantiate(newStructure.gameObject);
        structure = newObj.GetComponent<DGInteractable>();
        structure.Position = coord;
    }

    [Tooltip("This function is used for before the map has been rendered.")]
    public void AddItem(DGInteractable newItem)
    {
        item = newItem;
        item.Position = coord;
    }

    public bool hasBeenSearched;
    public float searchScore;
}

public class FloorRoom
{
    public TileCoord origin;
    public List<FloorRoom> connectedTo;
    public List<FloorRoom> connectedFrom;
    public FloorRoom()
    {
        connectedTo = new List<FloorRoom>();
        connectedFrom = new List<FloorRoom>();
    }

    public int length, height, padding;

    public int Bottom
    {
        get { return origin.z; }
    }
    public int BottomPadded
    {
        get { return Bottom - padding; }
    }
    public int Top
    {
        get { return origin.z + height - 1; }
    }
    public int TopPadded
    {
        get { return Top + padding; }
    }
    public int Left
    {
        get { return origin.x; }
    }
    public int LeftPadded
    {
        get { return Left - padding; }
    }
    public int Right
    {
        get { return origin.x + length - 1; }
    }
    public int RightPadded
    {
        get { return Right + padding; }
    }

    public bool isXOverlap(FloorRoom other)
    {
        return LeftPadded <= other.RightPadded && RightPadded >= other.LeftPadded;
    }
    public bool isZOverlap(FloorRoom other)
    {
        return BottomPadded <= other.TopPadded && TopPadded >= other.BottomPadded;
    }
    public bool isColliding(FloorRoom other)
    {
        return isXOverlap(other) && isZOverlap(other);
    }
    public bool IsCoordInRoom(TileCoord coord)
    {
        return coord.x >= Left && coord.x <= Right && coord.z >= Bottom && coord.z <= Top;
    }
    public bool AreSurroundingCoordsInRoom(TileCoord coord)
    {
        return IsCoordInRoom(coord.North) || IsCoordInRoom(coord.South) || IsCoordInRoom(coord.East) || IsCoordInRoom(coord.West);
    }

    public TileCoord GetRandomCoordInRoom()
    {
        return new TileCoord(UnityEngine.Random.Range(origin.x, origin.x + length), UnityEngine.Random.Range(origin.z, origin.z + height));
    }
}


[System.Serializable]
public class DungeonFloor
{
    public string floorName;
    public static readonly int floorSize = 50;
    public List<FloorRoom> rooms;
    public TileInfo[] tiles;
    public void ClearSearch()
    {
        foreach (var tile in tiles)
        {
            tile.hasBeenSearched = false;
            tile.searchScore = 0;
        }
    }

    public int CoordToIndex(TileCoord coord)
    {
        return (coord.z * floorSize) + coord.x;
    }
    public TileCoord IndexToCoord(int i)
    {
        int x = i % floorSize;
        int z = (i - x) / floorSize;
        return new TileCoord(x, z);
    }
    public TileInfo CoordToTileInfo(TileCoord coord)
    {
        if (!DungeonFloor.IsInX(coord.x) || !DungeonFloor.IsInZ(coord.z))
            return null;
        return tiles[CoordToIndex(coord)];
    }
    public TileInfo GetNextAvailable(TileCoord coord)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!tiles[i].isWall && tiles[i].occupyingEntity == null)
                return tiles[i];
        }
        return null;
    }
    static public bool IsInZ(int z)
    {
        return z >= 0 && z < floorSize;
    }
    static public bool IsInX(int x)
    {
        return x >= 0 && x < floorSize;
    }

    public void Fill()
    {
        tiles = new TileInfo[floorSize * floorSize];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new TileInfo(IndexToCoord(i));
            tiles[i].isWall = true;
        }
    }
}

[CreateAssetMenu(fileName = "DungeonData", menuName = "Scriptable Objects/Dungeon Data")]
public class DGData : ScriptableObject
{
    public bool isAscending = false;
    public int floorCount;
    public string dungeonName;
    public Tilesets.TILESET defaultTileset;
    public DGSeed floorSeed;
}