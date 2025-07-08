using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyDataEntry
{
    public string Key;
    public int Int;
    public float Float;
    public string String;
    public Object Obj;
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

public interface IDGInteraction : IYieldable
{
    public abstract bool Interact(DGEntity interacted, DGInteractable interactable, KeyDataList dataList); // Returns true if interaction was successful
}