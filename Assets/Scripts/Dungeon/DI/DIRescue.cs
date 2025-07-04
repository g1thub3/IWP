using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DIRescue", menuName = "Dungeon Interactions/DIRescue")]
public class DIRescue : SingletonScriptableObject<DIRescue>, IDGInteraction
{
    private IEnumerator QuestCoroutine(DGEntity interactable, DungeonUIHandler ui, DGGameManager dgGameManager, Quest q, bool playerCompleted)
    {
        string[] seq = {"Oh, thank you so much for rescuing me! Meet me back at the guild hall and I'll pass you your reward!"};
        DialogueData[] newData = { new DialogueData(seq) };
        GlobalCanvasManager.Instance.DialogueHandler.PromptSequence(newData);
        while (GlobalCanvasManager.Instance.DialogueHandler.IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        if (playerCompleted)
        {
            q.quest.questCompleted = true;
            q.quest.questPossible = false;
            ui.UpdateQuestUI();
            dgGameManager.QuestCompletePrompt();
        } else
        {
            q.quest.questPossible = false;
            ui.UpdateQuestUI();
            dgGameManager.QuestFailPrompt(q);
        }
        dgGameManager.RegisterRemoval(interactable);
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
                ui.AddEntry(interacted.gameObject.name + " has been rescued and has left the dungeon safely!");
                interacted.StartCoroutine(QuestCoroutine(intEntity, ui, dgGameManager, foundQuest, interacted is DGPlayer));
            }
        }
        return false;
    }
}
