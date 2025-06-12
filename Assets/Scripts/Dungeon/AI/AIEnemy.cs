using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AIEnemy", menuName = "Dungeon AI/AIEnemy")]
public class AIEnemy : DGAIModule
{
    public override void Run(DGNPC user, KeyDataList dataList = null)
    {
        DGGenerator generator = FindAnyObjectByType<DGGenerator>();
        DGEntity entity = user.GetComponent<DGEntity>();
        CharacterBehaviour cb = entity.GetComponent<CharacterBehaviour>();

        //// Get own room
        FloorRoom currRoom = null;
        for (int i = 0; i < entity.Floor.rooms.Count; i++)
        {
            if (entity.Floor.rooms[i].IsCoordInRoom(entity.Position))
            {
                currRoom = entity.Floor.rooms[i];
                break;
            }
        }

        // Get closest target
        List<TileCoord> path = new List<TileCoord>();
        CharacterBehaviour closestTarget = null;
        foreach (CharacterBehaviour member in generator.ActiveParty)
        {
            var newpath = entity.AStarPathfind(entity.Position, entity.GetClosestDirection(member.GetComponent<DGEntity>().Position));
            if (closestTarget != null)
            {
                if (newpath.Count > path.Count)
                {
                    continue;
                }
            }
            path = newpath;
            closestTarget = member;
        }
        if (closestTarget != null && path.Count > 0)
        {
            // Check if entity should target or not
            // Are they in the same room?
            FloorRoom targetRoom = null;
            for (int i = 0; i < closestTarget.GetComponent<DGEntity>().Floor.rooms.Count; i++)
            {
                if (closestTarget.GetComponent<DGEntity>().Floor.rooms[i].IsCoordInRoom(closestTarget.GetComponent<DGEntity>().Position))
                {
                    targetRoom = closestTarget.GetComponent<DGEntity>().Floor.rooms[i];
                    break;
                }
            }
            DebugTools.Instance.ClearMarkers();
            for (int i = 0; i < path.Count; i++)
            {
                DebugTools.Instance.AddMarker(TileInfo.CoordToPosition(path[i]), i.ToString());
            }
            // If entity is not in the room, are they close to each other and accessible?
            if (targetRoom == currRoom || path.Count < 5)
            {
                if (path.Count > 1)
                {
                    TileCoord diff = path[1] - entity.Position;
                    user.GetComponent<DGEntity>().Move(diff.x, diff.z);
                    Debug.Log("MOVED!");
                }
                else
                {
                    TileCoord diff = closestTarget.GetComponent<DGEntity>().Position - entity.Position;
                    user.GetComponent<DGEntity>().Move(diff.x, diff.z);
                    DefaultAttack.Instance.Perform(cb);
                    Debug.Log("ATTACKED!");
                }
                return;
            }
        }
        user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2));
        Debug.Log("RANDOM!");
    }
}
