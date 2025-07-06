using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "AIAlly", menuName = "Dungeon AI/AIAlly")]
public class AIAlly : SingletonScriptableObject<AIAlly>, DGAIModule
{

    public void Run(DGNPC user, KeyDataList dataList = null)
    {
        DGGenerator generator = FindAnyObjectByType<DGGenerator>();
        DGEntity entity = user.GetComponent<DGEntity>();
        CharacterBehaviour cb = entity.GetComponent<CharacterBehaviour>();

        //// Get own room
        FloorRoom currRoom = entity.CurrentRoom;

        // calculate dist from leader and dist from closest enemy
        // if enemy is closer prioritise that

        // Follow Leader
        CharacterBehaviour partyLeader = generator.ActiveParty[0];
        List<TileCoord> leaderPath = null;
        if (partyLeader != null)
        {
            leaderPath = entity.AStarPathfind(entity.Position, entity.GetClosestDirection(partyLeader.GetComponent<DGEntity>().Position));
        }

        CharacterBehaviour closestTarget = null;
        List<TileCoord> enemyPath = null;
        foreach (DGEntity member in generator.ActiveEntities)
        {
            if (member.GetComponent<CharacterBehaviour>().alliance == cb.alliance) continue;
            var newpath = entity.AStarPathfind(entity.Position, entity.GetClosestDirection(member.Position));
            if (newpath.Count == 0)
            {
                continue;
            }
            if (closestTarget != null)
            {
                if (newpath.Count > enemyPath.Count)
                {
                    continue;
                }
            }
            enemyPath = newpath;
            closestTarget = member.GetComponent<CharacterBehaviour>();
        }

        // Leader close, enemy far: follow leader
        // Leader far, enemy far: random
        // Leader far, enemy close: target enemy
        // Leader close, enemy close: target enemy


        bool isTargetingEnemy = false;
        bool isFollowingLeader = false;
        if (enemyPath != null)
        {
            if (leaderPath != null)
            {
                if (enemyPath.Count <= leaderPath.Count)
                {
                    isTargetingEnemy = true;
                } else
                {
                    isFollowingLeader = true;
                }
            } else
            {
                isTargetingEnemy = true;
            }
        } else
        {
            if (leaderPath != null)
                isFollowingLeader = true;
        }
        if (isTargetingEnemy)
        {
            if (enemyPath.Count < 5)
            {
                if (enemyPath.Count > 1)
                {
                    TileCoord diff = enemyPath[1] - entity.Position;
                    if (!user.GetComponent<DGEntity>().Move(diff.x, diff.z))
                    {
                        user.GetComponent<DGEntity>().Wait();
                    }
                    return;
                }
                else
                {
                    TileCoord diff = closestTarget.GetComponent<DGEntity>().Position - entity.Position;
                    user.GetComponent<DGEntity>().Move(diff.x, diff.z);
                    if (!cb.PerformMove(cb.defaultAttackInstance))
                    {
                        user.GetComponent<DGEntity>().Wait();
                    }
                }
                return;
            }
        }
        if (isFollowingLeader)
        {
            if (leaderPath.Count > 0 && leaderPath.Count < 25)
            {
                if (leaderPath.Count > 1)
                {
                    TileCoord diff = leaderPath[1] - entity.Position;
                    if (!user.GetComponent<DGEntity>().Move(diff.x, diff.z))
                    {
                        user.GetComponent<DGEntity>().Wait();
                    }
                }
                else
                {
                    user.GetComponent<DGEntity>().Wait();
                }
                return;
            }
        }
        if (!(user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2))))
        {
            user.GetComponent<DGEntity>().Wait();
        }
    }
}
