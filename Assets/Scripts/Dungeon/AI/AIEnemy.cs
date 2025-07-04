using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "AIEnemy", menuName = "Dungeon AI/AIEnemy")]
public class AIEnemy : SingletonScriptableObject<AIEnemy>, DGAIModule
{
    private static Dictionary<CharacterBehaviour, TileCoord> _plrPositions = new Dictionary<CharacterBehaviour, TileCoord>();
    public static void RemoveEntry(CharacterBehaviour behaviour)
    {
        _plrPositions.Remove(behaviour);
    }
    public static readonly int detectionRange = 5;
    public void Run(DGNPC user, KeyDataList dataList = null)
    {
        DGGenerator generator = FindAnyObjectByType<DGGenerator>();
        DGEntity entity = user.GetComponent<DGEntity>();
        CharacterBehaviour cb = entity.GetComponent<CharacterBehaviour>();

        //// Get own room
        FloorRoom currRoom = entity.CurrentRoom;
        // Get closest target

        foreach (CharacterBehaviour member in generator.ActiveParty)
        {
            float dist = member.GetComponent<DGEntity>().Position.DistanceSquared(entity.Position);
            if (dist > detectionRange * detectionRange && currRoom != member.GetComponent<DGEntity>().CurrentRoom)
                continue;
            TileCoord old = null;
            if (_plrPositions.ContainsKey(member))
            {
                old = _plrPositions[member];
            } else
            {
                _plrPositions.Add(member, member.GetComponent<DGEntity>().Position);
            }
            if (old == null || !old.Equals(member.GetComponent<DGEntity>().Position)) // If the target moved, track again
            {
                var newpath = entity.AStarPathfind(entity.Position, entity.GetClosestDirection(member.GetComponent<DGEntity>().Position));
                if (newpath.Count == 0)
                {
                    continue;
                }
                if (user.chaseTarget != null)
                {
                    if (newpath.Count > user.pathfind.Count)
                    {
                        continue;
                    }
                }
                user.pathfind = newpath;
                user.chaseTarget = member.GetComponent<DGObject>();
                _plrPositions[member] = member.GetComponent<DGEntity>().Position;
            }
        }
        if (user.chaseTarget != null && user.pathfind.Count > 0)
        {
            // Check if entity should target or not
            // Are they in the same room?
            FloorRoom targetRoom = null;
            for (int i = 0; i < user.chaseTarget.GetComponent<DGEntity>().Floor.rooms.Count; i++)
            {
                if (user.chaseTarget.GetComponent<DGEntity>().Floor.rooms[i].IsCoordInRoom(user.chaseTarget.GetComponent<DGEntity>().Position))
                {
                    targetRoom = user.chaseTarget.GetComponent<DGEntity>().Floor.rooms[i];
                    break;
                }
            }
            // If entity is not in the room, are they close to each other and accessible?
            if (targetRoom == currRoom || user.pathfind.Count < detectionRange)
            {
                if (user.pathfind.Count > 1)
                {
                    TileCoord diff = user.pathfind[1] - entity.Position;
                    if (!user.GetComponent<DGEntity>().Move(diff.x, diff.z))
                    {
                        user.GetComponent<DGEntity>().Wait();
                        return;
                    } else
                    {
                        user.pathfind.Remove(user.pathfind.First());
                        return;
                    }
                }
                else
                {
                    TileCoord diff = user.chaseTarget.GetComponent<DGEntity>().Position - entity.Position;
                    user.GetComponent<DGEntity>().Move(diff.x, diff.z);
                    if (!cb.PerformMove(DefaultAttack.Instance))
                    {
                        user.GetComponent<DGEntity>().Wait();
                    }
                }
                return;
            }
        }
        user.chaseTarget = null;
        if (!(user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2))))
        {
            user.GetComponent<DGEntity>().Wait();
        }
    }
}
