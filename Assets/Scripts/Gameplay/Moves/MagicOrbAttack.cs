using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "MagicOrbAttack", menuName = "Combat Moves/MagicOrbAttack")]
public class MagicOrbAttack : AttackMove
{
    public int shootRange = 4;
    public float projectileSpeed = 0.5f;
    public float waitTime = 0.25f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.faceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];

        _dungeonUI.AddEntry(user.gameObject.name + " attacked using Magic Orb!");

        Transition trans = new Transition();
        trans.max = waitTime;
        while (trans.Progression < 1)
        {
            trans.Progress();
            yield return new WaitForEndOfFrame();
        }

        TileInfo hitTile = null;
        CharacterBehaviour contact = null;
        for (int i = 0; i < shootRange; i++)
        {
            var pt = selfEntity.Position + (selfEntity.faceDir * (i + 1));
            hitTile = selfEntity.Floor.CoordToTileInfo(pt);
            contact = user.HitDetect(pt);
            if (hitTile.isWall || contact != null)
            {
                break;
            }
        }

        Vector3 startPos = selfTile.CoordToPosition();
        Vector3 endPos = hitTile.CoordToPosition();

        trans.t = 0;
        trans.max = (startPos - endPos).magnitude / projectileSpeed;
        GameObject orb = VFXManager.Instance.CreateLooped("orb_loop", startPos, 1);
        orb.transform.localScale *= 2;
        AudioManager.Instance.PlayFromObject(user.personalSource, "MagicOrb");
        while (trans.Progression < 1)
        {
            trans.Progress();
            orb.transform.position = Vector3.Lerp(startPos, endPos, trans.Progression);
            yield return new WaitForEndOfFrame();
        }

        var exp = VFXManager.Instance.Create("magicorb_impact_effect", endPos, 2);
        exp.transform.localScale *= 2;
        Destroy(orb);
        // attack
        if (contact != null)
        {
            contact.Damage(baseDamage, ATTACK_TYPE.MAGIC, user);
        } else
        {
            _dungeonUI.AddEntry("But nobody was hit...");
        }

        trans.t = 0;
        trans.max = waitTime;
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

        for (int i = 0; i < shootRange; i++)
        {
            var hitArea = selfEntity.Position + (selfEntity.faceDir * (i + 1));
            TileInfo tile = selfEntity.Floor.CoordToTileInfo(hitArea);
            if (tile.isWall)
                return false;
            var hitDetect = user.HitDetect(hitArea);
            if (hitDetect != null)
                return true;
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
