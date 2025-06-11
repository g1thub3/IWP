using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyDataEntry
{
    public string Key;
    public int Int;
    public float Float;
    public GameObject Obj;

    public KeyDataEntry(string mKey, int data)
    {
        Key = mKey;
        Int = data;
    }

    public KeyDataEntry(string mKey, float data)
    {
        Key = mKey;
        Float = data;
    }

    public KeyDataEntry(string mKey, GameObject obj) { 
        Key = mKey;
        Obj = obj;
    }
}

[System.Serializable]
public class KeyDataList
{
    public List<KeyDataEntry> dataList;
    public KeyDataList()
    {
        dataList = new List<KeyDataEntry>();
    }
    public KeyDataEntry GetData(string key)
    {
        foreach (KeyDataEntry entry in dataList)
        {
            if (entry.Key == key)
            {
                return entry;
            }
        }
        return null;
    }
}

public abstract class DGInteraction : SingletonScriptableObject<DGInteraction>
{
    protected bool _interactionInProgress = false;
    public bool InteractionInProgress
    {
        get { return _interactionInProgress; }
    }
    public abstract bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList); // Returns true if interaction was successful
}