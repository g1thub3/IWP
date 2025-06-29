using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DIRescue", menuName = "Dungeon Interactions/DIRescue")]
public class DIRescue : SingletonScriptableObject<DIRescue>, IDGInteraction
{
    private IEnumerator QuestCoroutine()
    {
        yield return null;
    }
    public bool IsInProgress()
    {
        return GlobalCanvasManager.Instance.IsInteractionActive;
    }
    public bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList)
    {
        DungeonUIHandler ui = FindAnyObjectByType<DungeonUIHandler>();
        DGGameManager dgGameManager = FindAnyObjectByType<DGGameManager>();
        if (interactable.TryGetComponent<DGEntity>(out DGEntity intEntity) && interactable.TryGetComponent<CharacterBehaviour>(out CharacterBehaviour cb))
        {
            Quest foundQuest = RescueQuest.CheckCompletion(dgGameManager, cb.character);
            if (foundQuest != null) {
                ui.UpdateQuestUI();
                ui.AddEntry(interacted.gameObject.name + " has been rescued and has left the dungeon safely!");
                foundQuest.quest.questCompleted = true;

                dgGameManager.QuestCompletePrompt();
            }
        }
        return false;
    }
}
