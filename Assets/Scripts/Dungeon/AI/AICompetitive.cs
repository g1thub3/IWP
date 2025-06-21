using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AICompetitive", menuName = "Dungeon AI/AICompetitive")]
public class AICompetitive : DGAIModule
{
    public override void Run(DGNPC user, KeyDataList dataList = null)
    {
        DGGenerator generator = FindAnyObjectByType<DGGenerator>();
        DGEntity entity = user.GetComponent<DGEntity>();
        CharacterBehaviour cb = entity.GetComponent<CharacterBehaviour>();


        
        user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2));
    }
}