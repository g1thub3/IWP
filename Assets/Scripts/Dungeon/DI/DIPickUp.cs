using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DIPickUp", menuName = "Dungeon Interactions/DIPickUp")]
public class DIPickUp : DGInteraction
{
    private IEnumerator WaitForAnswer(PromptHandler p, DGGameManager receiver, DungeonUIHandler ui)
    {
        while (p.IsPromptInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        if (p.TakeAnswer() == 0)
        {
            //progress floor
            receiver.QuestComplete();
        }
        _interactionInProgress = false;
        ui.UpdateQuestUI();
    }

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
                Quest foundQuest = null;
                DGGameManager dgGameManager = FindAnyObjectByType<DGGameManager>();
                for (int i = 0; i < dgGameManager.ActiveQuests.Count; i++)
                {
                    RetrievalQuest retrieveQuest = dgGameManager.ActiveQuests[i].quest as RetrievalQuest;
                    if (retrieveQuest != null)
                    {
                        if (retrieveQuest.ToRetrieve == container.Item)
                        {
                            foundQuest = dgGameManager.ActiveQuests[i];
                            foundQuest.quest.questCompleted = true;
                            break;
                        }
                    }
                }
                if (foundQuest != null) {
                    _interactionInProgress = true;
                    var p = GlobalCanvasManager.Instance.PromptHandler;

                    PromptInfo prompt = new PromptInfo();
                    prompt.message = "You completed a quest! Would you like to leave the dungeon now?";
                    prompt.options = new string[2];
                    prompt.options[0] = "Yes";
                    prompt.options[1] = "No";

                    CanvasGroup[] hidden = new CanvasGroup[1];
                    hidden[0] = ui.combatGrp;

                    p.Prompt(prompt, hidden);
                    interacted.StartCoroutine(WaitForAnswer(p, dgGameManager, ui));
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
