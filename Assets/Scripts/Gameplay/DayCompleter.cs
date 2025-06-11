using UnityEngine;

public class DayCompleter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GlobalGameManager.Instance.DayOver)
        {
            GlobalGameManager.Instance.DayOver = false;

            for (int i = GlobalGameManager.Instance.ownedQuests.Count - 1; i >= 0; i--) { 
                if (GlobalGameManager.Instance.ownedQuests[i].quest.questCompleted)
                {
                    GlobalGameManager.Instance.ownedQuests[i].goldReward.Award();
                    GlobalGameManager.Instance.ownedQuests[i].itemReward.Award();
                    GlobalGameManager.Instance.ownedQuests[i].adventurerReward.Award();
                    GlobalGameManager.Instance.ownedQuests.RemoveAt(i);
                }
            }

            for (int i = GlobalGameManager.Instance.inventory.Count - 1; i >= 0; i--)
            {
                if (GlobalGameManager.Instance.inventory[i].IsQuestTarget)
                {
                    GlobalGameManager.Instance.inventory.RemoveAt(i);
                }
            }
        }
        GlobalGameManager.Instance.CycleDay();
    }
}
