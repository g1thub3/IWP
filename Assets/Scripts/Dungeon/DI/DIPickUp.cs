using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DIPickUp", menuName = "Dungeon Interactions/DIPickUp")]
public class DIPickUp : DGInteraction
{

    public override bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList)
    {
        DGItemContainer container = interactable.GetComponent<DGItemContainer>();
        DungeonUIHandler ui = FindAnyObjectByType<DungeonUIHandler>();
        if (container == null)
            return false;
        if (interacted is DGPlayer && GlobalGameManager.Instance.inventory.Count < GlobalGameManager.inventoryLimit)
        {
            GlobalGameManager.Instance.AddItem(container.Item);
            ui.AddEntry(interacted.gameObject.name + " picked up a " + container.Item.module.itemName + (container.Item.IsQuestTarget ? " (Quest)" : string.Empty) + " and added it to your inventory.");

            if (interacted is DGPlayer && container.Item.IsQuestTarget)
            {
                DGGameManager dgGameManager = FindAnyObjectByType<DGGameManager>();
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
