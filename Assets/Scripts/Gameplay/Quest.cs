using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface IQuestReward
{
    public abstract void Award();
}

[System.Serializable]
public class GoldReward : IQuestReward
{
    int amount;
    public int Amount
    {
        get { return amount; }
    }
    public GoldReward(int amt)
    {
        amount = amt;
    }
    public void Award()
    {
        GlobalGameManager.Instance.ownedGold += amount;
    }
}

[System.Serializable]
public class ItemReward : IQuestReward
{
    List<Item> reward;
    public ItemReward(List<Item> reward)
    {
        this.reward = reward;
    }

    public void Award()
    {
        foreach (Item item in reward) { 
            GlobalGameManager.Instance.AddItem(item);
        }
    }
}

[System.Serializable]
public class AdventurerReward : IQuestReward {
    int amount;
    public int Amount
    {
        get { return amount; }
    }
    public AdventurerReward(int amt) {
        amount = amt;
    }
    public void Award() {
        GlobalGameManager.Instance.AddAdventurerEXP(amount);
    }
}

[System.Serializable]
public class Quest
{
    // WHO THE QUEST IS FROM
    public string clientName;
    public CHARACTER_ENUM clientCharacter;
    public bool isActive;
    public char difficulty;
    public void CalculateDifficulty()
    {
        difficulty = 'S';
        goldReward = new GoldReward(100);
        var itemData = new List<Item>();
        var item = new Item();
        item.itemKey = "Health Potion";
        item.Set();
        itemData.Add(item);
        itemReward = new ItemReward(itemData);
        adventurerReward = new AdventurerReward(200);
    }

    // WHAT THE QUEST WILL GIVE
    public GoldReward goldReward;
    public ItemReward itemReward;
    public AdventurerReward adventurerReward;
    public Quest(QuestData q, string cN, CHARACTER_ENUM chr, GoldReward gR = null, ItemReward iR = null, AdventurerReward aR = null)
    {
        clientName = cN;
        clientCharacter = chr;
        goldReward = gR;
        itemReward = iR;
        adventurerReward = aR;
        isActive = false;
        quest = q;
    }

    // WHAT THE QUEST IS (FOR DUNGEON)
    public QuestData quest;

    public enum QUEST_TYPE
    {
        RETRIEVAL
    }

    private static RetrievalQuest CreateRetrievalQuest()
    {
        var newQuest = new RetrievalQuest();
        newQuest.SetData();
        return newQuest;
    }

    public static Quest CreateQuestData(QUEST_TYPE qType)
    {
        QuestData newQuest;
        switch (qType)
        {
            default:
            case QUEST_TYPE.RETRIEVAL:
                newQuest = CreateRetrievalQuest();
                break;
        }
        var newData = new Quest(newQuest, CharacterProfiles.Instance.GetRandomNPCName(),(CHARACTER_ENUM)Random.Range(2, (int)CHARACTER_ENUM.NUM_CHARACTERS));
        newData.CalculateDifficulty();
        return newData;
    }


    public string QuestTitleText
    {
        get
        {
            if (quest is RetrievalQuest)
            {
                var rquest = quest as RetrievalQuest;
                return "Help " + clientName + " retrieve their " + rquest.ToRetrieve.module.itemName + "!";
            }
            return string.Empty;
        }
    }
    public string QuestClientText
    {
        get
        {
            return "Client: " + clientName;
        }
    }
    public string QuestPlaceText
    {
        get
        {
            string floor = string.Empty;
            if (quest.dungeon.isAscending)
                floor = quest.floor + "F";
            else
                floor = "B" + quest.floor + "F";
            return "Place: " + quest.dungeon.dungeonName + " " + floor;
        }
    }
    public string QuestObjectiveText
    {
        get
        {
            if (quest is RetrievalQuest)
            {
                var rquest = quest as RetrievalQuest;
                return "Objective: Retrieve " + rquest.ToRetrieve.module.itemName + ".";
            }
            return string.Empty;
        }
    }
    public string QuestDifficultyText
    {
        get
        {
            return "Difficulty: " + difficulty + " (" + adventurerReward.Amount + ")";
        }
    }
    public string QuestRewardText
    {
        get
        {
            string rewardText = "Reward: ";
            if (goldReward != null)
                rewardText += goldReward.Amount + " Gold | ";
            if (itemReward != null)
                rewardText += "??? | ";
            return rewardText;
        }
    }
}

[System.Serializable]
public abstract class QuestData
{
    public DGData dungeon;
    public int floor;
    public bool questCompleted; // NOTE: RESET THIS WHEN THE PLAYER LOSES IN THE DUNGEON
    public void SetData()
    {
        dungeon = GlobalGameManager.Instance.availableDungeons[Random.Range(0, GlobalGameManager.Instance.availableDungeons.Count)];
        floor = Random.Range(1, dungeon.floorCount);
        questCompleted = false;
    }

    public abstract void Execute();
    public abstract bool IsComplete();
}

[System.Serializable]
public class RetrievalQuest : QuestData
{
    public Item ToRetrieve;
    public RetrievalQuest()
    {
        ToRetrieve = new Item();
        if (Item.foundAssets == null)
            Item.FindAssets();
        ToRetrieve.module = Item.foundAssets[Random.Range(0, Item.foundAssets.Length)];
        ToRetrieve.Set();
    }
    public override void Execute() {
        // Place the item in the dungeon once floor entered
    }
    public override bool IsComplete()
    {
        // Trigger this with on pickup
        foreach (var item in GlobalGameManager.Instance.inventory)
        {
            if (item == ToRetrieve)
                return true;
        }
        return false;
    }
}
