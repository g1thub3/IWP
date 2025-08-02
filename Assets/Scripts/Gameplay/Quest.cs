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
        GlobalGameManager.Instance.AddGold(amount);
    }
}

[System.Serializable]
public class ItemReward : IQuestReward
{
    List<Item> reward;
    public List<Item> Reward {
        get { return reward; }
    }
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
        var data = GlobalGameManager.Instance.AddAdventurerEXP(amount);
        if (data["OldRank"] < data["NewRank"])
        {
            GlobalCanvasManager.Instance.LevelUpHandler.RankUpSequence(data);
        }
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
    public int competitiveLevel;
    public int diffPts;
    public void CalculateDifficulty()
    {
        diffPts = quest.dungeon.floorDifficulty * quest.floor + (quest.dungeon.floorDifficulty * quest.floor * competitiveLevel);
        var itemData = new List<Item>();
        if (diffPts < 15)
        {
            difficulty = 'S';
            goldReward = new GoldReward(100);
            itemData.Add(Item.New("Health Potion"));
            itemData.Add(Item.New("Adventurer Meal"));
            adventurerReward = new AdventurerReward(200);
        } else if (diffPts < 60)
        {
            difficulty = 'A';
            goldReward = new GoldReward(150);
            itemData.Add(Item.New("Warding Rune"));
            itemData.Add(Item.New("Steel Plating"));
            adventurerReward = new AdventurerReward(300);
        }
        else if (diffPts < 120)
        {
            difficulty = 'B';
            goldReward = new GoldReward(450);
            itemData.Add(Item.New("Magic Charm"));
            itemData.Add(Item.New("Spiked Band"));
            adventurerReward = new AdventurerReward(400);
        }
        else if (diffPts < 200)
        {
            difficulty = 'C';
            goldReward = new GoldReward(600);
            itemData.Add(Item.New("Clairvoyant Lenses"));
            adventurerReward = new AdventurerReward(500);
        }
        itemReward = new ItemReward(itemData);
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
        RETRIEVAL,
        RESCUE,
        NUM_QUEST_TYPES
    }

    public static Quest CreateQuestData(QUEST_TYPE qType, bool isComp = false)
    {
        QuestData newQuest;
        switch (qType)
        {
            default:
            case QUEST_TYPE.RETRIEVAL:
                newQuest = new RetrievalQuest();
                break;
            case QUEST_TYPE.RESCUE:
                newQuest = new RescueQuest();
                break;
        }
        var newData = new Quest(newQuest, CharacterProfiles.Instance.GetRandomNPCName(),(CHARACTER_ENUM)Random.Range(2, (int)CHARACTER_ENUM.NUM_CHARACTERS));
        if (isComp)
        {
            int compLevel = 1 + (Random.Range(0, GlobalGameManager.Instance.adventurerRanking));
            compLevel = Mathf.Min(newQuest.dungeon.floorDifficulty, compLevel);
            newData.competitiveLevel = compLevel;
        }
        else
        {
            newData.competitiveLevel = 0;
        }
        newData.CalculateDifficulty();
        return newData;
    }


    public string QuestTitleText
    {
        get
        {
            return quest.GetTitle(this);
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
            return quest.GetObjective(this);
        }
    }
    public string QuestDifficultyText
    {
        get
        {
            return "Difficulty: " + difficulty + " (" + adventurerReward.Amount + ")";
        }
    }
    public string QuestCompetitiveLevelText
    {
        get
        {
            return "Competitive Level: " + competitiveLevel;
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

    private static Dictionary<string, DGData> _dungeons;
    private static void SearchDungeons()
    {
        if (_dungeons == null)
        {
            DGData[] resources = Resources.LoadAll<DGData>("Dungeons");
            if (resources != null)
            {
                _dungeons = new Dictionary<string, DGData>();
                foreach (var data in resources)
                {
                    _dungeons.Add(data.dungeonName, data);
                }
            }
        }
    }
    private static DGData GetDungeon(string name)
    {
        SearchDungeons();
        if (_dungeons != null)
            return _dungeons[name];
        return null;
    }

    public DGData dungeon;
    public int floor;
    public bool questPossible;
    public bool questCompleted; // NOTE: RESET THIS WHEN THE PLAYER LOSES IN THE DUNGEON
    public QuestData()
    {
        dungeon = GlobalGameManager.Instance.availableDungeons[Random.Range(0, GlobalGameManager.Instance.availableDungeons.Count)];
        floor = Random.Range(1, dungeon.floorCount);
        questCompleted = false;
        questPossible = true;
    }

    public abstract string GetTitle(Quest info);
    public abstract string GetObjective(Quest info);

    public abstract DGObject Execute(Quest info, DGGenerator dungeonGen);
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
    public override DGObject Execute(Quest info, DGGenerator dungeonGen) {
        // Place the item in the dungeon once floor entered
        var room = dungeonGen.GetRandomRoom();
        var spawnTile = dungeonGen.SearchRandomTileInRoom(room, SearchConditions.New(false));
        if (spawnTile != null)
        {
            var questItem = Tilesets.Instance.ConstructItemInteractable(ToRetrieve);
            questItem.GetComponent<DGItemContainer>().Item.IsQuestTarget = true;
            dungeonGen.InsertItem(questItem, spawnTile);
            return spawnTile.item;
        }
        return null;
    }
    public override string GetTitle(Quest info)
    {
        return "Help " + info.clientName + " retrieve their " + ToRetrieve.module.itemName + "!";
    }
    public override string GetObjective(Quest info)
    {
        return "Objective: Retrieve " + ToRetrieve.module.itemName + ".";
    }

    public static Quest CheckCompletion(DGGameManager gameManager, Item pickedUp)
    {
        for (int i = 0; i < gameManager.ActiveQuests.Count; i++)
        {
            RetrievalQuest retrieveQuest = gameManager.ActiveQuests[i].quest as RetrievalQuest;
            if (retrieveQuest != null)
            {
                if (retrieveQuest.ToRetrieve == pickedUp)
                {
                    var foundQuest = gameManager.ActiveQuests[i];
                    foundQuest.quest.questCompleted = true;
                    return foundQuest;
                }
            }
        }
        return null;
    }
}

[System.Serializable]
public class RescueQuest : QuestData
{
    public CharacterEntry ToRescue;
    public RescueQuest()
    {
        ToRescue = CharacterEntry.Create(CharacterProfiles.Instance.GetRandomEnum(), 5);
    }
    public override DGObject Execute(Quest info, DGGenerator dungeonGen)
    {
        // Place the item in the dungeon once floor entered
        var newNPC = dungeonGen.SpawnNPC(DG_CHARACTER_TYPE.QUEST, ToRescue);
        newNPC.gameObject.name = info.clientName;
        return newNPC.GetComponent<DGObject>();
    }

    public override string GetTitle(Quest info)
    {
        return "Rescue " + info.clientName + " from " + dungeon.dungeonName + "!";
    }
    public override string GetObjective(Quest info)
    {
        return "Objective: Rescue " + info.clientName + ".";
    }

    public static Quest CheckCompletion(DGGameManager gameManager, CharacterEntry rescued)
    {
        for (int i = 0; i < gameManager.ActiveQuests.Count; i++)
        {
            RescueQuest rescueQuest = gameManager.ActiveQuests[i].quest as RescueQuest;
            if (rescueQuest != null)
            {
                if (rescueQuest.ToRescue == rescued)
                {
                    var foundQuest = gameManager.ActiveQuests[i];
                    foundQuest.quest.questCompleted = true;
                    return foundQuest;
                }
            }
        }
        return null;
    }
}
