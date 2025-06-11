using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

[CreateAssetMenu(fileName = "BasicSeed", menuName = "Dungeon Seeds/Basic Seed")]
public class BasicSeed : DGSeed
{
    [Range(0, 24)]
    public int roomSpawnBounds;

    [Range(1, 10)]
    public int roomPadding; // Distance between rooms

    public int minRoomCount;
    public int maxRoomCount;
    public int minRoomX;
    public int maxRoomX;
    public int minRoomZ;
    public int maxRoomZ;

    public uint minEnemies;
    public uint maxEnemies;

    private bool _pathSuccessful;

    public List<NPCGenData> possibleEnemies;

    public override DungeonFloor Generate(DGData dungeonData)
    {
        DebugTools.Instance.ClearMarkers();
        DungeonFloor floorData = new DungeonFloor();
        floorData.Fill();

        int roomCount = Random.Range(minRoomCount, maxRoomCount);

        List<FloorRoom> rooms = new List<FloorRoom>();

        // Create the rooms
        // Minor issue: The fail safe causes this to generate less than the minimum number of rooms
        for (int i = 0; i < roomCount; i++)
        {
            FloorRoom newRoom = new FloorRoom();
            newRoom.padding = roomPadding;

            //distance check from other rooms


            bool safePlacement = false;
            int failSafe = 0;
            while (!safePlacement)
            {
                //pick a start point
                newRoom.origin = new TileCoord(Random.Range(roomSpawnBounds, DungeonFloor.floorSize - roomSpawnBounds),
                    Random.Range(roomSpawnBounds, DungeonFloor.floorSize - roomSpawnBounds));
                newRoom.length = Random.Range(minRoomX, maxRoomX);
                newRoom.height = Random.Range(minRoomZ, maxRoomZ);

                List<FloorRoom> collidingRooms = new List<FloorRoom>();
                for (int rI = 0; rI < rooms.Count; rI++)
                {
                    FloorRoom room = rooms[rI];
                    if (room == null) break;
                    if (newRoom.isColliding(room))
                    {
                        collidingRooms.Add(room);
                    }
                }
                safePlacement = collidingRooms.Count == 0;
                failSafe++;
                if (failSafe > 100)
                {
                    Debug.Log("FAIL SAFE TRIGGERED.");
                    break;
                }
            }
            if (!safePlacement)
            {
                break;
            }

            //extend
            for (int col = 0; col < newRoom.length; col++)
            {
                for (int row = 0; row < newRoom.height; row++)
                {
                    TileCoord coord = new TileCoord(col, row) + newRoom.origin;
                    if (DungeonFloor.IsInZ(coord.z) && DungeonFloor.IsInX(coord.x))
                    {
                        floorData.tiles[floorData.CoordToIndex(coord)].isWall = false;
                    }
                }
            }
            rooms.Add(newRoom);

            //if (DebugTools.Instance.DungeonDebugOn)
            //{
            //    DebugTools.Instance.AddMarker(new Vector2(newRoom.LeftPadded * TileInfo.tileScale, newRoom.BottomPadded * TileInfo.tileScale));
            //    DebugTools.Instance.AddMarker(new Vector2(newRoom.LeftPadded * TileInfo.tileScale, newRoom.TopPadded * TileInfo.tileScale));
            //    DebugTools.Instance.AddMarker(new Vector2(newRoom.RightPadded * TileInfo.tileScale, newRoom.BottomPadded * TileInfo.tileScale));
            //    DebugTools.Instance.AddMarker(new Vector2(newRoom.RightPadded * TileInfo.tileScale, newRoom.TopPadded * TileInfo.tileScale));
            //}
        }

        floorData.rooms = rooms;

        // Connect the rooms
        for (int i = 0; i < rooms.Count; i++)
        {
            FloorRoom room = floorData.rooms[i];

            // Select rooms to connect to

            int connectionCount = Random.Range(1, floorData.rooms.Count);
            for (int j = 0; j < connectionCount;)
            {
                int roomIndex = Random.Range(0, floorData.rooms.Count);
                if (roomIndex == i)
                {
                    // Don't connect to yourself
                    continue;
                }
                j++;
                FloorRoom chosenRoom = floorData.rooms[roomIndex];
                room.connectedTo.Add(chosenRoom);
                chosenRoom.connectedFrom.Add(room);
            }

            // Create paths to the connected rooms
            foreach (FloorRoom connectedRoom in room.connectedTo)
            {
                bool completed = false;
                while (!completed)
                {
                    // Decide a side to exit from
                    int failSafe = 0;
                    TileCoord start = null;
                    while (start == null)
                    {
                        if (failSafe == 50)
                            break;
                        int XRand = Random.Range(room.origin.x, room.origin.x + room.length);
                        int YRand = Random.Range(room.origin.z, room.origin.z + room.height);

                        int alongX = Random.Range(0, 2);
                        if (alongX == 1)
                        {
                            TileCoord found = new TileCoord(XRand, Random.Range(0, 2) == 0 ? room.Bottom - 1 : room.Top + 1);
                            if (floorData.tiles[floorData.CoordToIndex(found)].isWall
                                && floorData.tiles[floorData.CoordToIndex(found.East)].isWall
                                && floorData.tiles[floorData.CoordToIndex(found.West)].isWall)
                            {
                                start = found;
                            }
                        }
                        else
                        {
                            TileCoord found = new TileCoord(Random.Range(0, 2) == 0 ? room.Left - 1 : room.Right + 1, YRand);
                            if (floorData.tiles[floorData.CoordToIndex(found)].isWall
                                && floorData.tiles[floorData.CoordToIndex(found.North)].isWall
                                && floorData.tiles[floorData.CoordToIndex(found.South)].isWall)
                            {
                                start = found;
                            }
                        }
                        failSafe++;
                    }
                    if (start == null)
                        break;
                    failSafe = 0;
                    // Decide a side to enter from
                    TileCoord end = null;
                    while (end == null)
                    {
                        if (failSafe == 50)
                            break;
                        int XRand = Random.Range(connectedRoom.origin.x, connectedRoom.origin.x + connectedRoom.length);
                        int YRand = Random.Range(connectedRoom.origin.z, connectedRoom.origin.z + connectedRoom.height);

                        int alongX = Random.Range(0, 2);
                        if (alongX == 1)
                        {
                            TileCoord found = new TileCoord(XRand, Random.Range(0, 2) == 0 ? connectedRoom.Bottom - 1 : connectedRoom.Top + 1);
                            if (floorData.tiles[floorData.CoordToIndex(found)].isWall)
                            {
                                end = found;
                            }
                        }
                        else
                        {
                            TileCoord found = new TileCoord(Random.Range(0, 2) == 0 ? connectedRoom.Left - 1 : connectedRoom.Right + 1, YRand);
                            if (floorData.tiles[floorData.CoordToIndex(found)].isWall)
                            {
                                end = found;
                            }
                        }
                        failSafe++;
                    }

                    if (end == null)
                        break;

                    List<TileCoord> path = PathCreateAStar(start, end, room, connectedRoom, floorData);

                    if (_pathSuccessful)
                    {
                        foreach (TileCoord pathCoord in path)
                        {
                            floorData.tiles[floorData.CoordToIndex(pathCoord)].isWall = false;
                        }
                        completed = true;
                    }
                }
            }
        }

        FloorRoom selectedStaircaseRoom = floorData.rooms[Random.Range(0, floorData.rooms.Count)];
        floorData.tiles[floorData.CoordToIndex(selectedStaircaseRoom.GetRandomCoordInRoom())]
            .AddStructure(Tilesets.Instance.structureList.GetData("BasementStairs").Obj.GetComponent<DGInteractable>());

        for (int i = 0; i < 3; i++)
        {
            floorData.tiles[floorData.CoordToIndex(selectedStaircaseRoom.GetRandomCoordInRoom())]
                .AddItem(Tilesets.Instance.ConstructItemInteractable("Health Potion").GetComponent<DGInteractable>());
        } 

        return floorData;
    }

    private bool IsPathPossible(TileCoord target, TileCoord prev, TileCoord end, FloorRoom startRoom, FloorRoom endRoom)
    {
        // CONDITIONS:
        // Can't be in startRoom
        // Can't be in endRoom unless it is the end
        // Can't be the previous
        // Has to be in bounds

        return !startRoom.AreSurroundingCoordsInRoom(target) 
            && !(endRoom.AreSurroundingCoordsInRoom(target) && !target.Equals(end)) 
            && !target.Equals(prev) 
            && DungeonFloor.IsInZ(target.z) 
            && DungeonFloor.IsInX(target.x);
    }

    public class TrackPoint
    {
        public TrackPoint(TileCoord newPt, TileCoord endPt, int newID)
        {
            pt = newPt;
            dist = newPt.DistanceSquared(endPt);
            points = 1;
            id = newID;

        }
        public TileCoord pt;
        public float dist;
        public int points;
        public int id;
    }

    private List<TileCoord> PathCreateAStar(TileCoord start, TileCoord end, FloorRoom startRoom, FloorRoom endRoom, DungeonFloor floorData)
    {
        _pathSuccessful = false;
        // GOALS
        // Prioritise tiles that aren't walls so the paths join together instead of clumping up
        // Straighten out the paths instead of diagonals

        List<TileCoord> path = new List<TileCoord>();
        TileCoord prev = null;
        TileCoord curr = start;

        if (DebugTools.Instance.DungeonDebugOn)
        {
            DebugTools.Instance.AddMarker(new Vector2(start.x * TileInfo.tileScale, start.z * TileInfo.tileScale));
            DebugTools.Instance.AddMarker(new Vector2(end.x * TileInfo.tileScale, end.z * TileInfo.tileScale));
        }

        int goodID = -1;
        int pathMax = 0;
        while (!_pathSuccessful)
        {
            if (DebugTools.Instance.DungeonDebugOn)
            {
                //DebugTools.Instance.AddMarker(new Vector2(curr.x * TileInfo.tileScale, curr.z * TileInfo.tileScale));
            }
            path.Add(curr);
            if (curr.Equals(end))
            {
                path.Add(end);
                _pathSuccessful = true;
                break;
            }
            if (pathMax >= 1000)
                break;

            TrackPoint up = new TrackPoint(curr.North, end, 0);
            TrackPoint right = new TrackPoint(curr.East, end, 1);
            TrackPoint down = new TrackPoint(curr.South, end, 2);
            TrackPoint left = new TrackPoint(curr.West, end, 3);

            List<TrackPoint> pts = new List<TrackPoint>();
            pts.Add(up); pts.Add(right);
            pts.Add(down); pts.Add(left);
            for (int i = 0; i < pts.Count; i++)
            {
                for (int j = 0; j < pts.Count; j++)
                {
                    if (pts[i].dist < pts[j].dist)
                    {
                        pts[i].points++;
                        //var temp = pts[i];
                        //pts[i] = pts[j];
                        //pts[j] = temp;
                    }
                }
            }
            for (int i = 0; i < pts.Count; i++)
            {
                if (!floorData.tiles[floorData.CoordToIndex(pts[i].pt)].isWall)
                {
                    pts[i].points *= 10;
                }
            }
            for (int i = 0; i < pts.Count; i++)
            {
                if (pts[i].id == goodID && pts[i].dist < prev.DistanceSquared(end))
                {
                    pts[i].points *= 100;
                }
            }
            for (int i = pts.Count - 1; i >= 0; i--)
            {
                if (!IsPathPossible(pts[i].pt, prev, end, startRoom, endRoom))
                {
                    pts[i].points = -1;
                    //pts.RemoveAt(i);
                }
            }
            for (int i = pts.Count - 1; i >= 0; i--)
            {
                foreach (FloorRoom room in floorData.rooms)
                {
                    if (room.AreSurroundingCoordsInRoom(pts[i].pt) && room != startRoom && room != endRoom)
                    {
                        pts[i].points = -1;
                        break;
                    }
                }
            }

            int chosen = -1;
            int highest = 0;
            for (int i = 0; i < pts.Count; i++)
            {
                if (highest < pts[i].points)
                {
                    highest = pts[i].points;
                    chosen = i;
                }
            }

            if (chosen == -1)
            {
                Debug.Log("Failed to find direction.");
                if (DebugTools.Instance.DungeonDebugOn)
                    DebugTools.Instance.AddMarker(new Vector2(curr.x * TileInfo.tileScale, curr.z * TileInfo.tileScale), Color.blue);
                break;
            }

            prev = curr;
            goodID = pts[chosen].id;
            curr = pts[chosen].pt;
            pathMax++;
        }
        if (!_pathSuccessful)
        {
            if (DebugTools.Instance.DungeonDebugOn)
                DebugTools.Instance.AddMarker(new Vector2(curr.x * TileInfo.tileScale, curr.z * TileInfo.tileScale), Color.yellow);
            Debug.Log("Failed to pathfind.");
        }
        return path;
    }

    public override void AddEnemies(DGGenerator dgGen)
    {
        int enemyCount = Random.Range((int)minEnemies, (int)maxEnemies);
        for (int i = 0; i < enemyCount; i++)
        {
            int enemyIndex = Random.Range(0, possibleEnemies.Count);
            CharacterEntry newCharacter = CharacterEntry.Create(possibleEnemies[enemyIndex].character, 
                Random.Range((int)possibleEnemies[enemyIndex].minLevel, (int)possibleEnemies[enemyIndex].maxLevel));
            dgGen.SpawnNPC(newCharacter);
        }
    }
}