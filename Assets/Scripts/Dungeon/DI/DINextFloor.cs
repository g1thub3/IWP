using NUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DINextFloor", menuName = "Dungeon Interactions/DINextFloor")]
public class DINextFloor : SingletonScriptableObject<DINextFloor>, IDGInteraction
{
    public bool IsInProgress()
    {
        return GlobalCanvasManager.Instance.PromptHandler.IsInProgress();
    }

    private IEnumerator WaitForAnswer(PromptHandler p, DGGameManager receiver)
    {
        while (p.IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        if (p.TakeAnswer() == 0)
        {
            //progress floor
            AudioManager.Instance.PlaySFXInScreen("Stairs");
            receiver.ToNextFloor();
        }
    }
    public bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList)
    {
        DungeonUIHandler ui = FindAnyObjectByType<DungeonUIHandler>();
        DGGameManager receiver = FindAnyObjectByType<DGGameManager>();
        if (interacted.TryGetComponent<DGNPC>(out DGNPC npcmod))
        {
            if (npcmod.associatedCompetitor != null) {
                if (interacted.GetComponent<CharacterBehaviour>().character == npcmod.associatedCompetitor.party[0])
                {
                    if (interactable as DGObject == npcmod.associatedCompetitor.target)
                    {
                        // Despawn them, increase the competitor floor
                        npcmod.associatedCompetitor.currentFloor++;
                        for (int i = npcmod.associatedCompetitor.partySpawned.Count - 1; i >= 0; i--)
                        {
                            receiver.RegisterRemoval(npcmod.associatedCompetitor.partySpawned[i]);
                        }
                        ui.AddEntry(npcmod.associatedCompetitor.competitorName + " has found the exit and has gone to the next floor!");

                    }
                }
            }
        }
        if (!(interacted is DGPlayer))
        {
            return false;
        }
        DungeonFloor floorData = interacted.Floor;
        var p = GlobalCanvasManager.Instance.PromptHandler;
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
