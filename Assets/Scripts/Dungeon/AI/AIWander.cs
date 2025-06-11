using UnityEngine;

[CreateAssetMenu(fileName = "AIWander", menuName = "Dungeon AI/AIWander")]
public class AIWander : DGAIModule
{
    public override void Run(DGNPC user, KeyDataList dataList = null)
    {
        user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2));
    }
}
