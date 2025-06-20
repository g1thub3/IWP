using System;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    public string itemName;
    public string itemDescription;
    public Sprite itemSprite;
    public bool isConsumable;

    public virtual void ApplyEffect(CharacterEntry holder) { }
    public virtual void RemoveEffect(CharacterEntry holder) { }
    public virtual void Use(CharacterBehaviour user) { }
}

[System.Serializable]
public class Item
{
    public string itemKey;
    public ItemData module;
    public static ItemData[] foundAssets;

    private bool _isQuestTarget = false;
    public bool IsQuestTarget
    {
        get { return _isQuestTarget; }
        set { _isQuestTarget = value; }
    }

    public static void FindAssets()
    {
        foundAssets = Resources.LoadAll<ItemData>("");
    }

    public override string ToString()
    {
        if (module != null)
        {
            return module.itemName;
        } else
        {
            return "None";
        }
    }

    public void Implement()
    {
        if (foundAssets == null)
            FindAssets();
        // Search for item in Resources
        foreach (ItemData item in foundAssets)
        {
            if (item.itemName.Equals(itemKey))
            {
                module = item;
                break;
            }
        }
    }
    public void Set()
    {
        if (itemKey != null && module == null)
        {
            Implement();
        } else if (module != null && itemKey == null)
        {
            itemKey = module.itemName;
        }
    }

    public static Item New(string key)
    {
        var item = new Item();
        item.itemKey = key;
        item.Set();
        return item;
    }
    public static Item New(ItemData mod)
    {
        var item = new Item();
        item.module = mod;
        item.itemKey = mod.itemName;
        return item;
    }
}