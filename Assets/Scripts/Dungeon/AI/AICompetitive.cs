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
        if (user.associatedCompetitor.target != null)
        {
            List<TileCoord> path = new List<TileCoord>();
            if (user.associatedCompetitor.target is DGEntity)
            {
                var target = user.associatedCompetitor.target as DGEntity;
                path = entity.AStarPathfind(entity.Position, entity.GetClosestDirection(target.Position));
            }
            else if (user.associatedCompetitor.target is DGInteractable)
            {
                path = entity.AStarPathfind(entity.Position, user.associatedCompetitor.target.Position);
            }
            if (path.Count > 1)
            {
                TileCoord diff = path[1] - entity.Position;
                entity.Move(diff.x, diff.z);
                return;
            } else
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
        if (!(user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2))))
        {
            entity.Wait();
        }
    }
}