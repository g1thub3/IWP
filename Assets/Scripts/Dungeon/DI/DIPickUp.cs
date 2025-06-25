using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DIPickUp", menuName = "Dungeon Interactions/DIPickUp")]
public class DIPickUp : DGInteraction
{

    public override bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList)
    {
        DGItemContainer container = interactable.GetComponent<DGItemContainer>();
        DungeonUIHandler ui = FindAnyObjectByType<DungeonUIHandler>();
        DGGameManager dgGameManager = FindAnyObjectByType<DGGameManager>();
        if (container == null)
            return false;
        if (interacted is DGPlayer && GlobalGameManager.Instance.inventory.Count < GlobalGameManager.inventoryLimit)
        {
            GlobalGameManager.Instance.AddItem(container.Item);
            ui.AddEntry(interacted.gameObject.name + " picked up a " + container.Item.module.itemName + (container.Item.IsQuestTarget ? " (Quest)" : string.Empty) + " and added it to your inventory.");

            if (interacted is DGPlayer && container.Item.IsQuestTarget)
            {
                Quest foundQuest = RetrievalQuest.CheckCompletion(dgGameManager, container.Item);
                if (foundQuest != null) {
                    ui.UpdateQuestUI();
                    foundQuest.quest.questCompleted = true;
                    dgGameManager.QuestCompletePrompt();
                }
            }
            return true;
        }

        var cb = interacted.GetComponent<CharacterBehaviour>();
        if (cb == null)
            return false;

        if (interacted.TryGetComponent<DGNPC>(out DGNPC npcmod))
        {
            if (npcmod.associatedCompetitor != null)
            {
                if (interactable as DGObject == npcmod.associatedCompetitor.target)
                {
                    // set quest false, destroy party
                    npcmod.associatedCompetitor.associatedQuest.quest.questPossible = false;
                    ui.AddEntry(npcmod.associatedCompetitor.competitorName + " retrieved the " + container.Item.module.itemName + "!");
                    dgGameManager.QuestFailPrompt(npcmod.associatedCompetitor.associatedQuest);
                    return true;
                }
                return false;
            }
        }

        if (cb.character.HeldItem != null)
        {
            ui.AddEntry(interacted.gameObject.name + " passed over a " + container.Item.module.itemName + (container.Item.IsQuestTarget ? " (Quest)" : string.Empty) + ".");
            return false;
        } else
        {
            cb.character.HeldItem = container.Item;
            ui.AddEntry(interacted.gameObject.name + " picked up a " + container.Item.module.itemName + (container.Item.IsQuestTarget ? " (Quest)" : string.Empty) + ".");
            return true;
        }
    }
}
