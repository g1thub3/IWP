using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class DGInteractable : DGObject
{
    public KeyDataList dataList;
    public IDGInteraction interaction;
    public bool DestroyOnInteract = false;
    public bool CanInteractWithAction = false;

    private IEnumerator WaitForCompletion()
    {
        while (interaction.IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    public void OnInteract(DGEntity interacted, DungeonFloor floorData)
    {
        bool completed = interaction.Interact(interacted, this, dataList);
        if (DestroyOnInteract && completed)
        {
            StartCoroutine(WaitForCompletion());
        }
    }
    protected new void Start()
    {
        base.Start();
        GlobalCanvasManager.LoadInstance();
    }
}
