using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "DefaultAttack", menuName = "Combat Moves/DefaultAttack")]
public class DefaultAttack : AttackMove
{

    public float moveTime1 = 0.3f;
    public float moveTime2 = 0.15f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.FaceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];
        var hitTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(hitArea)];
        Vector3 startPos = selfTile.CoordToPosition();
        Vector3 endPos = hitTile.CoordToPosition();

        _dungeonUI.AddEntry(user.gameObject.name + " attacked!");

        Transition trans = new Transition();
        trans.max = moveTime1;
        while (trans.Progression < 1)
        {
            trans.Progress();
            user.transform.position = Vector3.Lerp(startPos, endPos, trans.Progression);
            yield return new WaitForEndOfFrame();
        }
        // attack
        if (WillMoveSucceed(user))
        {
            var detected = user.HitDetect(hitArea);
            if (detected != null)
            {
                detected.Damage(baseDamage, ATTACK_TYPE.PHYSICAL, user);
            }
        }
        Transition trans2 = new Transition();
        trans2.max = moveTime2;
        while (trans2.Progression < 1) {
            trans2.Progress();
            user.transform.position = Vector3.Lerp(endPos, startPos, trans2.Progression);
            yield return new WaitForEndOfFrame();
        }
        _dgGameManager.TurnCompleted.Invoke();
        user.GetComponent<DGEntity>().IsPerformingAction = false;
    }

    public override bool WillMoveSucceed(CharacterBehaviour user)
    {
        var selfEntity = user.GetComponent<DGEntity>();

        var hitArea = selfEntity.Position + selfEntity.FaceDir;
        if (user.HitDetect(hitArea) == null)
            return false;

        TileCoord xDiff = new TileCoord(selfEntity.Position.x + selfEntity.FaceDir.x, selfEntity.Position.z);
        TileCoord zDiff = new TileCoord(selfEntity.Position.x, selfEntity.Position.z + selfEntity.FaceDir.z);

        TileInfo tile = selfEntity.Floor.CoordToTileInfo(hitArea);
        TileInfo xTile = selfEntity.Floor.CoordToTileInfo(xDiff);
        TileInfo zTile = selfEntity.Floor.CoordToTileInfo(zDiff);
        if (tile.isWall || xTile.isWall || zTile.isWall)
        {
            return false;
        }
        return true;
    }

    public override bool Perform(CharacterBehaviour user)
    {
        base.Perform(user);

        DGGameManager _dgGameManager = FindAnyObjectByType<DGGameManager>();
        DungeonUIHandler _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        DGEntity entity = user.GetComponent<DGEntity>();
        entity.IsPerformingAction = true;
        entity.StartCoroutine(MoveAnimation(user, _dgGameManager, _dungeonUI));
        return true;
    }
}
