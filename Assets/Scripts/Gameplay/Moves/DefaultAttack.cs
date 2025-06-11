using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

[CreateAssetMenu(fileName = "DefaultAttack", menuName = "Combat Moves/DefaultAttack")]
public class DefaultAttack : CombatMove
{
    public float moveTime1 = 0.3f;
    public float moveTime2 = 0.15f;
    private IEnumerator MoveAnimation(CharacterBehaviour user, DGGameManager _dgGameManager, DungeonUIHandler _dungeonUI)
    {
        var selfEntity = user.GetComponent<DGEntity>();
        var hitArea = selfEntity.Position + selfEntity.faceDir;
        var selfTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(selfEntity.Position)];
        var hitTile = selfEntity.Floor.tiles[selfEntity.Floor.CoordToIndex(hitArea)];
        Vector3 startPos = selfTile.CoordToPosition();
        Vector3 endPos = hitTile.CoordToPosition();

        Transition trans = new Transition();
        trans.max = moveTime1;
        while (trans.Progression < 1)
        {
            trans.Progress();
            user.transform.position = Vector3.Lerp(startPos, endPos, trans.Progression);
            yield return new WaitForEndOfFrame();
        }
        // attack
        _dungeonUI.AddEntry(user.gameObject.name + " attacked!");
        var detected = user.HitDetect(hitArea);
        if (detected != null)
        {
            detected.Damage(15, ATTACK_TYPE.PHYSICAL, user.character);
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

    public override bool Perform(CharacterBehaviour user)
    {
        DGGameManager _dgGameManager = FindAnyObjectByType<DGGameManager>();
        DungeonUIHandler _dungeonUI = FindAnyObjectByType<DungeonUIHandler>();
        DGEntity entity = user.GetComponent<DGEntity>();
        entity.IsPerformingAction = true;
        entity.StartCoroutine(MoveAnimation(user, _dgGameManager, _dungeonUI));
        return true;
    }
}
