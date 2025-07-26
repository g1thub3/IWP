using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "BodySlamAttack", menuName = "Combat Moves/BodySlamAttack")]
public class BodySlamAttack : AttackMove
{

    public float moveTime1 = 0.25f;
    public float moveTime2 = 0.15f;
    public float moveTime3 = 0.45f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.faceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];
        var hitTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(hitArea)];
        Vector3 startPos = selfTile.CoordToPosition();
        Vector3 endPos = hitTile.CoordToPosition();
        Vector3 endPosUp = endPos + new Vector3(0,0.75f,0);

        _dungeonUI.AddEntry(user.gameObject.name + " attacked using Body Slam!");

        Transition trans = new Transition();
        trans.max = moveTime1;
        while (trans.Progression < 1)
        {
            trans.Progress();
            user.transform.position = Vector3.Lerp(startPos, endPosUp, trans.Progression);
            yield return new WaitForEndOfFrame();
        }
        trans.t = 0;
        trans.max = moveTime2;
        while (trans.Progression < 1)
        {
            trans.Progress();
            user.transform.position = Vector3.Lerp(endPosUp, endPos, trans.Progression);
            yield return new WaitForEndOfFrame();
        }
        // attack
        AudioManager.Instance.PlayFromObject(user.personalSource, "BodySlam");
        VFXManager.Instance.Create("bodyslam_effect", endPos, 1);
        if (WillMoveSucceed(user))
        {
            var detected = user.HitDetect(hitArea);
            if (detected != null)
            {
                detected.Damage(baseDamage, ATTACK_TYPE.PHYSICAL, user);
            }
        }
        trans.t = 0;
        trans.max = moveTime2;
        while (trans.Progression < 1) {
            trans.Progress();
            user.transform.position = Vector3.Lerp(endPos, startPos, trans.Progression);
            yield return new WaitForEndOfFrame();
        }
        _dgGameManager.TurnCompleted.Invoke();
        user.GetComponent<DGEntity>().IsPerformingAction = false;
    }

    public override bool WillMoveSucceed(CharacterBehaviour user)
    {
        var selfEntity = user.GetComponent<DGEntity>();

        var hitArea = selfEntity.Position + selfEntity.faceDir;
        if (user.HitDetect(hitArea) == null)
            return false;
        TileCoord xDiff = new TileCoord(selfEntity.Position.x + selfEntity.faceDir.x, selfEntity.Position.z);
        TileCoord zDiff = new TileCoord(selfEntity.Position.x, selfEntity.Position.z + selfEntity.faceDir.z);

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
