using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "JabAttack", menuName = "Combat Moves/JabAttack")]
public class JabAttack : AttackMove
{

    private float jabWindup = 0.25f;

    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.faceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];
        var hitTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(hitArea)];
        Vector3 endPos = hitTile.CoordToPosition();

        _dungeonUI.AddEntry(user.gameObject.name + " attacked using Jab!");
        selfEntity._action = ANIMATION_ENUM.JAB;

        Transition trans = new Transition();
        trans.max = jabWindup;
        while (trans.Progression < 1)
        {
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }

        VFXManager.Instance.Create("jab_effect", endPos, 1);

        // attack
        if (WillMoveSucceed(user))
        {
            var detected = user.HitDetect(hitArea);
            if (detected != null)
            {
                detected.Damage(baseDamage, ATTACK_TYPE.PHYSICAL, user);
            }
        }

        trans.t = 0;
        trans.max = (1 - jabWindup);
        while (trans.Progression < 1)
        {
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }
        selfEntity._action = ANIMATION_ENUM.IDLE;
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
