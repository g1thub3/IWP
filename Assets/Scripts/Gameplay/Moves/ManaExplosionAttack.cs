using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "ManaExplosionAttack", menuName = "Combat Moves/ManaExplosionAttack")]
public class ManaExplosionAttack : AttackMove
{
    private float castUpswing = 0.4f;
    public float waitTime = 0.3f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.FaceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];
        var hitTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(hitArea)];
        Vector3 startPos = selfTile.CoordToPosition();

        _dungeonUI.AddEntry(user.gameObject.name + " attacked using Mana Explosion!");
        AudioManager.Instance.PlayFromObject(user.personalSource, "Charge1");

        selfEntity._action = ANIMATION_ENUM.CAST;
        Transition trans = new Transition();
        trans.max = castUpswing;
        while (trans.Progression < 1)
        {
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }

        AudioManager.Instance.PlayFromObject(user.personalSource, "Explosion");
        var obj = VFXManager.Instance.Create("manaexplosion_effect", startPos, 1);
        obj.transform.localScale *= 3f;

        // attack
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                TileCoord hitCoord = selfEntity.Position + new TileCoord(i, j);
                var detected = user.HitDetect(hitCoord);
                if (detected != null)
                {
                    detected.Damage(baseDamage, ATTACK_TYPE.MAGIC, user);
                }
            }
        }

        trans.max = 1 - castUpswing;
        trans.t = 0;
        while (trans.Progression < 1) {
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }
        selfEntity._action = ANIMATION_ENUM.IDLE;
        trans.max = waitTime;
        trans.t = 0;
        while (trans.Progression < 1)
        {
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }
        _dgGameManager.TurnCompleted.Invoke();
        user.GetComponent<DGEntity>().IsPerformingAction = false;
    }

    public override bool WillMoveSucceed(CharacterBehaviour user)
    {
        var selfEntity = user.GetComponent<DGEntity>();

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                TileCoord hitCoord = selfEntity.Position + new TileCoord(i, j);
                var detected = user.HitDetect(hitCoord);
                if (detected != null)
                    return true;
            }
        }
        return false;
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
