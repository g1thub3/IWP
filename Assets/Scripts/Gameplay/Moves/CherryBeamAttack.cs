using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "CherryBeamAttack", menuName = "Combat Moves/CherryBeamAttack")]
public class CherryBeamAttack : AttackMove
{
    public GameObject cherryBeam;
    public GameObject runeCircle;

    public float beamTime = 0.15f;
    public float tweenLength = 0.6f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        Transition transition = new Transition();
        _dungeonUI.AddEntry(user.gameObject.name + " attacked with Cherry Beam!");
        var selfEntity = user.GetComponent<DGEntity>();
        var extentArea = selfEntity.Position + selfEntity.FaceDir;
        Vector3 selfPosition = TileInfo.CoordToPosition(selfEntity.Position);
        Vector3 extPosition = TileInfo.CoordToPosition(extentArea);
        var circle = Instantiate(runeCircle, 
            Vector3.Lerp(selfPosition, extPosition, 0.35f), 
            Quaternion.Euler(0, 0, Mathf.Atan2(selfEntity.FaceDir.z, selfEntity.FaceDir.x) * Mathf.Rad2Deg - 90.0f));
        circle.transform.localScale = Vector3.zero;

        AudioManager.Instance.PlayFromObject(user.personalSource, "Charge2");
        circle.GetComponent<RuneCircle>().Scale(1, tweenLength);
        while (circle.GetComponent<RuneCircle>().IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        transition.max = 1.0f;
        while (transition.Progression < 1)
        {
            transition.Progress();
            yield return new WaitForEndOfFrame();
        }

        AudioManager.Instance.PlayFromObject(user.personalSource, "Laser");
        transition.max = beamTime;
        var beam = Instantiate(cherryBeam, extPosition, Quaternion.Euler(0,0, Mathf.Atan2(selfEntity.FaceDir.z, selfEntity.FaceDir.x) * Mathf.Rad2Deg));
        var beamrenderer = beam.GetComponent<SpriteRenderer>();
        TileCoord curr = extentArea;
        while (true)
        {
            Vector3 currPos = TileInfo.CoordToPosition(curr);
            if (!(DungeonFloor.IsInZ(curr.z) && DungeonFloor.IsInX(curr.x)))
            {
                var vfx = VFXManager.Instance.Create("cherrybeam_impact_effect", currPos, 3);
                vfx.GetComponent<SpriteRenderer>().material.color = Color.red;
                break;
            }
            var currTile = selfEntity.Floor.CoordToTileInfo(curr);
            if (currTile.isWall)
            {
                var vfx = VFXManager.Instance.Create("cherrybeam_impact_effect", currPos, 3);
                vfx.GetComponent<SpriteRenderer>().material.color = Color.red;
                break;
            }
            var detected = user.HitDetect(curr);
            if (detected != null)
            {
                var vfx = VFXManager.Instance.Create("cherrybeam_impact_effect", currPos, 3);
                vfx.GetComponent<SpriteRenderer>().material.color = Color.red;
                detected.Damage(baseDamage, ATTACK_TYPE.MAGIC, user);
            }
            curr += selfEntity.FaceDir;
            var nextPos = TileInfo.CoordToPosition(curr);
            var currMagnitude = beamrenderer.size.x;
            var magnitude = (extPosition - nextPos).magnitude;
            var halfPoint = Vector3.Lerp(extPosition, nextPos, 0.5f);

            var currBeamPos = beam.transform.position;
            transition.t = 0;
            while (transition.Progression < 1)
            {
                transition.Progress();
                beam.transform.position = Vector3.Lerp(currBeamPos, halfPoint, transition.Progression);
                beamrenderer.size = new Vector2(0.75f + currMagnitude + (magnitude - currMagnitude) * transition.Progression,0.75f);
                yield return new WaitForEndOfFrame();
            }
        }

        transition.max = 0.5f;
        transition.t = 0;
        while (transition.Progression < 1)
        {
            transition.Progress();
            yield return new WaitForEndOfFrame();
        }

        transition.max = 0.25f;
        transition.t = 0;
        while (transition.Progression < 1)
        {
            transition.Progress();
            beamrenderer.size = new Vector2(beamrenderer.size.x, 0.75f * (1 - transition.Progression));
            yield return new WaitForEndOfFrame();
        }
        Destroy(beam);
        circle.GetComponent<RuneCircle>().Scale(0, 0.3f);
        while (circle.GetComponent<RuneCircle>().IsInProgress())
            yield return new WaitForEndOfFrame();
        Destroy(circle);

        _dgGameManager.TurnCompleted.Invoke();
        user.GetComponent<DGEntity>().IsPerformingAction = false;
    }

    public override bool WillMoveSucceed(CharacterBehaviour user)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        TileCoord curr = selfEntity.Position;
        while (true)
        {
            curr += selfEntity.FaceDir;
            if (!(DungeonFloor.IsInZ(curr.z) && DungeonFloor.IsInX(curr.x)))
                break;
            var tile = selfEntity.Floor.CoordToTileInfo(curr);
            if (tile.isWall) 
                break;
            var detected = user.HitDetect(curr);
            if (detected != null)
            {
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
