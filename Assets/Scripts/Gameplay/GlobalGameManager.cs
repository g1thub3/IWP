using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Quest;

[CreateAssetMenu(fileName = "GlobalGameManager", menuName = "Scriptable Objects/GlobalGameManager")]
public class GlobalGameManager : SingletonScriptableObject<GlobalGameManager>
{
    [Header("Game Data")]
    public static int adventurerRankMax = 30;
    public int adventurerEXP;
    public int adventurerRanking;
    public static int maxGold = 9999999;
    public int ownedGold;
    public int bankGold;

    public int WalletCapacity
    {
        get
        {
            return maxGold - ownedGold;
        }
    }
    public int BankCapacity
    {
        get
        {
            return maxGold - bankGold;
        }
    }

    public void AddGold(int amt)
    {
        ownedGold = Mathf.Clamp(ownedGold + amt, 0, maxGold);
    }

    public Dictionary<string,int> AddAdventurerEXP(int amt) // returns changes
    {
        Dictionary<string, int> data = new Dictionary<string, int>();
        data.Add("OldEXP", adventurerEXP);
        data.Add("OldRank", adventurerRanking);
        data.Add("AddedEXP", amt);
        adventurerEXP += amt;

        while (adventurerRanking < adventurerRankMax && adventurerEXP >= GetExpToNextRank())
        {
            adventurerEXP -= GetExpToNextRank();
            adventurerRanking++;
            if (adventurerRanking == adventurerRankMax)
            {
                adventurerEXP = 0;
            }
        }
        data.Add("NewRank", adventurerRanking);

        return data;
    }
    public int GetExpToNextRank(int rank = -1)
    {
        if (rank == -1)
            rank = adventurerRanking;
        if (rank == adventurerRankMax) return 0;
        return ((rank * 200) * rank);
    }
    
    public static int partyLimit = 4;
    public List<CharacterEntry> party = new List<CharacterEntry>();

    public static int inventoryLimit = 20;
    public List<Item> inventory = new List<Item>();

    public int storageLimit = 100;
    public List<Item> storage = new List<Item>();

    public static int shopLimit = 14;
    public List<Item> merchantShop = new List<Item>();
    public List<Item> armouryShop = new List<Item>();

    public void AddItem(Item newItem)
    {
        inventory.Add(newItem);
    }

    public bool PurchaseItem(Item newItem)
    {
        if (inventory.Count >= inventoryLimit) return false;
        if (ownedGold < newItem.module.ShopPrice) return false;
        AddItem(newItem);
        ownedGold -= newItem.module.ShopPrice;
        return true;
    }
    public void SellItem(int index)
    {
        ownedGold += inventory[index].module.SellValue;
        inventory.RemoveAt(index);
    }

    public void StoreItem(int index)
    {
        if (storage.Count >= storageLimit) return;
        storage.Add(inventory[index]);
        inventory.RemoveAt(index);
    }
    public void RetrieveItem(int index)
    {
        if (inventory.Count >= inventoryLimit) return;
        inventory.Add(storage[index]);
        storage.RemoveAt(index);
    }

    public List<DGData> availableDungeons = new List<DGData>();
    public DGData selectedDungeon;

    public void UseItem(int itemIndex, CharacterBehaviour affected)
    {
        var item = inventory[itemIndex];
        DungeonUIHandler dui = FindAnyObjectByType<DungeonUIHandler>();
        if (dui)
            dui.AddEntry(affected.gameObject.name + " used a " + item.module.itemName + "!");
        item.module.Use(affected);
        if (item.module.isConsumable)
            inventory.RemoveAt(itemIndex);
    }
    public bool HoldItem(int itemIndex, int partyIndex)
    {
        var selectedCharacter = party[partyIndex];
        if (selectedCharacter.HeldItem != null && selectedCharacter.HeldItem.module != null)
        {
            if (inventory.Count >= inventoryLimit)
                return false;
            AddItem(selectedCharacter.HeldItem);
            selectedCharacter.HeldItem = null;
        }
        selectedCharacter.HeldItem = inventory[itemIndex];
        inventory.Remove(selectedCharacter.HeldItem);
        return true;
    }

    public static int maxQuestCount = 8;
    public List<Quest> ownedQuests = new List<Quest>();
    public List<Quest> availableQuests = new List<Quest>();
    public List<Quest> availableCompetitiveQuests = new List<Quest>();

    public bool DayOver = false;

    public void CycleDay()
    {
        availableQuests.Clear();
        availableCompetitiveQuests.Clear();
        int max = (int)QUEST_TYPE.NUM_QUEST_TYPES;
        for (int i = 0; i < maxQuestCount; i++)
        {
            QUEST_TYPE rand = (QUEST_TYPE)Random.Range(0, max);
            var newQuest = Quest.CreateQuestData(rand);
            availableQuests.Add(newQuest);
        }
        for (int i = 0; i < availableQuests.Count; i++)
        {
            for (int j = i; j < availableQuests.Count; j++)
            {
                if (availableQuests[i].diffPts > availableQuests[j].diffPts)
                {
                    var temp = availableQuests[i];
                    availableQuests[i] = availableQuests[j];
                    availableQuests[j] = temp;
                }
            }
        }
        for (int i = 0; i < maxQuestCount; i++)
        {
            QUEST_TYPE rand = (QUEST_TYPE)Random.Range(0, max);
            var newQuest = Quest.CreateQuestData(rand, true);
            availableCompetitiveQuests.Add(newQuest);
        }
        for (int i = 0; i < availableCompetitiveQuests.Count; i++)
        {
            for (int j = i; j < availableCompetitiveQuests.Count; j++)
            {
                if (availableCompetitiveQuests[i].diffPts > availableCompetitiveQuests[j].diffPts)
                {
                    var temp = availableCompetitiveQuests[i];
                    availableCompetitiveQuests[i] = availableCompetitiveQuests[j];
                    availableCompetitiveQuests[j] = temp;
                }
            }
        }

        merchantShop.Clear();
        armouryShop.Clear();
        for (int i = 0; i < 14; i++)
        {
            merchantShop.Add(Item.New(Tilesets.Instance.merchantItems[Random.Range(0, Tilesets.Instance.merchantItems.Count)]));
            armouryShop.Add(Item.New(Tilesets.Instance.armouryItems[Random.Range(0, Tilesets.Instance.armouryItems.Count)]));
        }
        for (int i = 0; i < merchantShop.Count; i++)
        {
            for (int j = i; j < merchantShop.Count; j++)
            {
                if (merchantShop[j].module.ShopPrice < merchantShop[i].module.ShopPrice)
                {
                    var temp = merchantShop[j];
                    merchantShop[j] = merchantShop[i];
                    merchantShop[i] = temp;
                }
            }
        }
        for (int i = 0; i < armouryShop.Count; i++)
        {
            for (int j = i; j < armouryShop.Count; j++)
            {
                if (armouryShop[j].module.ShopPrice < armouryShop[i].module.ShopPrice)
                {
                    var temp = armouryShop[j];
                    armouryShop[j] = armouryShop[i];
                    armouryShop[i] = temp;
                }
            }
        }
    }

    private void OnEnable()
    {
        CycleDay();
        ownedQuests.Clear();
    }
}
