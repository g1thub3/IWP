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
    public List<CharacterEntry> party;

    public static int inventoryLimit = 20;
    public List<Item> inventory;

    public int storageLimit = 100;
    public List<Item> storage;

    public static int shopLimit = 14;
    public List<Item> merchantShop;
    public List<Item> armouryShop;

    public void AddItem(Item newItem)
    {
        if (inventory.Count >= inventoryLimit) return;
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

    public List<DGData> availableDungeons;
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
    public List<Quest> ownedQuests;
    public List<Quest> availableQuests;
    public List<Quest> availableCompetitiveQuests;

    public bool DayOver = false;

    public void CycleDay()
    {
        if (availableQuests == null)
            availableQuests = new List<Quest>();
        else
            availableQuests.Clear();

        if (availableCompetitiveQuests == null)
            availableCompetitiveQuests = new List<Quest>();
        else
            availableCompetitiveQuests.Clear();


        if (merchantShop == null)
            merchantShop = new List<Item>();
        else
            merchantShop.Clear();
        if (armouryShop == null)
            armouryShop = new List<Item>();
        else
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

        if (availableDungeons.Count < 1)
            return;
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
    }

    private void Default()
    {
        var cherry = CharacterEntry.Create(CHARACTER_ENUM.CHERRY, 5);
        var damson = CharacterEntry.Create(CHARACTER_ENUM.DAMSON, 5);
        party.Add(cherry);
        party.Add(damson);

        AddItem(Item.New("Health Potion"));
        AddItem(Item.New("Health Potion"));
        AddItem(Item.New("Health Potion"));

        ownedGold = 200;
        bankGold = 500;
        adventurerRanking = 1;
        adventurerEXP = 0;
    }

    private void OnEnable()
    {
        if (ownedQuests == null)
            ownedQuests = new List<Quest>();
        else
            ownedQuests.Clear();
        selectedDungeon = null;

        if (availableDungeons == null)
            availableDungeons = new List<DGData>();
        else
            availableDungeons.Clear();

        if (storage == null)
            storage = new List<Item>();
        else
            storage.Clear();

        if (inventory == null)
            inventory = new List<Item>();
        else
            inventory.Clear();

        if (party == null)
            party = new List<CharacterEntry>();
        else
            party.Clear();

        Default();

        CycleDay();
    }
}
