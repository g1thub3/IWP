using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "SingeAttack", menuName = "Combat Moves/SingeAttack")]
public class SingeAttack : AttackMove
{

    public float moveTime1 = 0.3f;
    public float waitTime = 0.15f;
    public float moveTime2 = 0.3f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.faceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];
        var hitTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(hitArea)];
        Vector3 startPos = selfTile.CoordToPosition();
        Vector3 endPos2 = hitTile.CoordToPosition();
        Vector3 endPos = Vector3.Lerp(startPos, endPos2, 0.25f);

        _dungeonUI.AddEntry(user.gameObject.name + " attacked using Singe!");

        Transition trans = new Transition();
        trans.max = moveTime1;
        while (trans.Progression < 1)
        {
            trans.Progress();
            user.transform.position = Vector3.Lerp(startPos, endPos, trans.Progression);
            yield return new WaitForEndOfFrame();
        }
        // attack
        VFXManager.Instance.Create("singe_effect", endPos2, 1);
        if (WillMoveSucceed(user))
        {
            var detected = user.HitDetect(hitArea);
            if (detected != null)
            {
                detected.Damage(baseDamage, ATTACK_TYPE.MAGIC, user);
            }
        }
        trans.t = 0;
        trans.max = waitTime;
        while (trans.Progression < 1)
        {
            trans.Progress();
            yield return new WaitForEndOfFrame();
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
        TileInfo tile = selfEntity.Floor.CoordToTileInfo(hitArea);
        if (tile.isWall)
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
