using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DINextFloor", menuName = "Dungeon Interactions/DINextFloor")]
public class DINextFloor : DGInteraction
{
    private IEnumerator WaitForAnswer(PromptHandler p, DGGameManager receiver)
    {
        while (p.IsPromptInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        if (p.TakeAnswer() == 0)
        {
            //progress floor
            receiver.RefreshGame();
        }
        _interactionInProgress = false;
    }
    public override bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList)
    {
        if (!(interacted is DGPlayer))
        {
            return false;
        }
        _interactionInProgress = true;
        DungeonFloor floorData = interacted.Floor;
        var p = GlobalCanvasManager.Instance.PromptHandler;
        DGGameManager receiver = FindAnyObjectByType<DGGameManager>();
        DungeonUIHandler ui = FindAnyObjectByType<DungeonUIHandler>();
        if (receiver != null)
        {
            PromptInfo prompt = new PromptInfo();
            prompt.message = "Would you like to go to the next floor?";
            prompt.options = new string[2];
            prompt.options[0] = "Yes";
            prompt.options[1] = "No";

            CanvasGroup[] hidden = null;
            if (ui != null)
            {
                hidden = new CanvasGroup[1];
                hidden[0] = ui.combatGrp;
            }
            p.Prompt(prompt, hidden);
            interacted.StartCoroutine(WaitForAnswer(p, receiver));
        }
        return true;
    }
}
