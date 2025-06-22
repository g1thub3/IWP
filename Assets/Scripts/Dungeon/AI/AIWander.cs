using UnityEngine;

[CreateAssetMenu(fileName = "AIWander", menuName = "Dungeon AI/AIWander")]
public class AIWander : SingletonScriptableObject<AIWander>, DGAIModule
{
    public void Run(DGNPC user, KeyDataList dataList = null)
    {
        if (!(user.GetComponent<DGEntity>().Move(Random.Range(-1, 2), Random.Range(-1, 2))))
        {
            user.GetComponent<DGEntity>().Wait();
        }
    }
}
