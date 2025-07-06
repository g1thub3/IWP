using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AICompetitive", menuName = "Dungeon AI/AICompetitive")]
public class AICompetitive : SingletonScriptableObject<AICompetitive>, DGAIModule
{
    public void Run(DGNPC user, KeyDataList dataList = null)
    {
        DGGenerator generator = FindAnyObjectByType<DGGenerator>();
        DGEntity entity = user.GetComponent<DGEntity>();
        CharacterBehaviour cb = entity.GetComponent<CharacterBehaviour>();
        if (user.associatedCompetitor == null)
        {
            if (!(user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2))))
            {
                entity.Wait();
            }
            return;
        }

        // Go after target
        List<TileCoord> targetPath = new List<TileCoord>();
        if (user.associatedCompetitor.target != null)
        {
            if (user.associatedCompetitor.target is DGEntity)
            {
                var target = user.associatedCompetitor.target as DGEntity;
                targetPath = entity.AStarPathfind(entity.Position, entity.GetClosestDirection(target.Position));
            }
            else if (user.associatedCompetitor.target is DGInteractable)
            {
                targetPath = entity.AStarPathfind(entity.Position, user.associatedCompetitor.target.Position);
            }
        }

        CharacterBehaviour closestTarget = null;
        List<TileCoord> enemyPath = null;
        foreach (DGEntity member in generator.ActiveEntities)
        {
            if (member.GetComponent<CharacterBehaviour>().alliance == cb.alliance || member.GetComponent<CharacterBehaviour>().alliance == -1) continue; // Ignore our own + rescue targets
            if (member.TryGetComponent<DGNPC>(out DGNPC memberNPC)) // Only target those with the same quest
            {
                if (memberNPC.associatedCompetitor != null)
                {
                    if (memberNPC.associatedCompetitor.associatedQuest != user.associatedCompetitor.associatedQuest)
                        continue;
                }
            }

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

        bool isChasingTarget = false;
        bool isTargetingOpponent = false;

        if (targetPath != null)
        {
            if (enemyPath != null)
            {
                if (targetPath.Count <= enemyPath.Count)
                {
                    isChasingTarget = true;
                } else
                {
                    isTargetingOpponent = true;
                }
            } else
            {
                isChasingTarget = true;
            }
        } else
        {
            if (enemyPath != null)
                isTargetingOpponent = true;
        }

        if (isChasingTarget)
        {
            if (targetPath.Count > 1)
            {
                TileCoord diff = targetPath[1] - entity.Position;
                entity.Move(diff.x, diff.z);
                return;
            }
            else
            {
                if (user.associatedCompetitor.target is DGEntity)
                {
                    var target = user.associatedCompetitor.target as DGEntity;
                    if (entity.GetClosestDirection(target.Position, true).Equals(entity.Position))
                    {
                        TileCoord diff = target.Position - entity.Position;
                        entity.FaceDirection(diff.x, diff.z);
                        if (!entity.InteractAction())
                        {
                            entity.Wait();
                        }
                        return;
                    }
                }
            }
        }
        if (isTargetingOpponent)
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
        if (!(user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2))))
        {
            entity.Wait();
        }
    }
}