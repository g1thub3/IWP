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
    public int questID;
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

    public QUEST_TYPE QuestType
    {
        get {
            if (quest is RetrievalQuest)
                return QUEST_TYPE.RETRIEVAL;
            if (quest is RescueQuest)
                return QUEST_TYPE.RESCUE;
            return QUEST_TYPE.NUM_QUEST_TYPES;
        }
    }

    public static Quest CreateQuestData(QUEST_TYPE qType, bool isComp = false)
    {
        string cliName = CharacterProfiles.Instance.GetRandomNPCName();
        CHARACTER_ENUM cliChar = CharacterProfiles.Instance.GetRandomEnum();
        QuestData newQuest;
        switch (qType)
        {
            default:
            case QUEST_TYPE.RETRIEVAL:
                newQuest = new RetrievalQuest();
                break;
            case QUEST_TYPE.RESCUE:
                newQuest = new RescueQuest(cliName, cliChar);
                break;
        }
        var newData = new Quest(newQuest, cliName, cliChar);
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
        int newID = Random.Range(100000, 1000000);
        bool found = false;
        while (!found)
        {
            found = true;
            foreach (var q in GlobalGameManager.Instance.ownedQuests)
            {
                if (q.questID == newID)
                {
                    found = false;
                    break;
                }
            }
            foreach (var q in GlobalGameManager.Instance.availableQuests)
            {
                if (q.questID == newID || found == false)
                {
                    found = false;
                    break;
                }
            }
            foreach (var q in GlobalGameManager.Instance.availableCompetitiveQuests)
            {
                if (q.questID == newID || found == false)
                {
                    found = false;
                    break;
                }
            }
            if (found) break;
            newID = Random.Range(100000, 1000000);
        }
        newData.questID = newID;
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
    protected DGObject target;
    public KeyDataList dataList;
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
        dataList = new KeyDataList();
    }

    public abstract string GetTitle(Quest info);
    public abstract string GetObjective(Quest info);

    public abstract DGObject Execute(Quest info, DGGenerator dungeonGen);
    public void SetTarget(DGObject newTarget) => target = newTarget;
    public virtual KeyDataList GetData() {
        dataList = new KeyDataList();
        return dataList;
    }
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

    public RetrievalQuest(string key)
    {
        ToRetrieve = Item.New(key);
    }
    public override DGObject Execute(Quest info, DGGenerator dungeonGen) {
        // Place the item in the dungeon once floor entered
        TileInfo spawnTile = null;
        if (dataList.GetData("X") != null && dataList.GetData("Z") != null)
        {
            spawnTile = dungeonGen.CurrentFloor.CoordToTileInfo(new TileCoord(dataList.GetData("X").Int, dataList.GetData("Z").Int));
        } else
        {
            var room = dungeonGen.GetRandomRoom();
            spawnTile = dungeonGen.SearchRandomTileInRoom(room, SearchConditions.New(false));
        }
        if (spawnTile != null)
        {
            var questItem = Tilesets.Instance.ConstructItemInteractable(ToRetrieve);
            questItem.GetComponent<DGItemContainer>().Item.IsQuestTarget = true;
            dungeonGen.InsertItem(questItem, spawnTile);
            target = spawnTile.item;
            return target;
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

    public override KeyDataList GetData()
    {
        base.GetData();
        dataList.dataList.Add(KeyDataEntry.ConstructString("Key",ToRetrieve.itemKey));
        if (target != null)
        {
            dataList.dataList.Add(KeyDataEntry.ConstructInt("X", target.Position.x));
            dataList.dataList.Add(KeyDataEntry.ConstructInt("Z", target.Position.z));
        }
        return dataList;
    }
}

[System.Serializable]
public class RescueQuest : QuestData
{
    public CharacterEntry ToRescue;
    public RescueQuest(string clientName, CHARACTER_ENUM clientCharacter)
    {
        ToRescue = CharacterEntry.Create(clientCharacter, 5);
        ToRescue.characterName = clientName;
    }
    public override DGObject Execute(Quest info, DGGenerator dungeonGen)
    {
        // Place the item in the dungeon once floor entered
        TileCoord point = null;
        if (dataList.GetData("X") != null && dataList.GetData("Z") != null)
        {
            point = new TileCoord(dataList.GetData("X").Int, dataList.GetData("Z").Int);
        }
        var newNPC = dungeonGen.SpawnNPC(DG_CHARACTER_TYPE.QUEST, ToRescue, point);
        newNPC.gameObject.name = info.clientName;
        if (dataList.GetData("HP") != null && dataList.GetData("HG") != null && dataList.GetData("EN") != null && dataList.GetData("MN") != null)
        {
            var cb = newNPC.GetComponent<CharacterBehaviour>();
            cb.health = dataList.GetData("HP").Int;
            cb.hunger = dataList.GetData("HG").Int;
            cb.energy = dataList.GetData("EN").Int;
            cb.mana = dataList.GetData("MN").Int;
        }
        if (dataList.GetData("FX") != null && dataList.GetData("FZ") != null)
        {
            newNPC.GetComponent<DGEntity>().FaceDir = new TileCoord(dataList.GetData("FX").Int, dataList.GetData("FZ").Int);
        }
        target = newNPC.GetComponent<DGObject>();
        return target;
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

    public override KeyDataList GetData()
    {
        base.GetData();
        if (target != null)
        {
            dataList.dataList.Add(KeyDataEntry.ConstructInt("X", target.Position.x));
            dataList.dataList.Add(KeyDataEntry.ConstructInt("Z", target.Position.z));
            var cb = target.GetComponent<CharacterBehaviour>();
            dataList.dataList.Add(KeyDataEntry.ConstructInt("HP", cb.health));
            dataList.dataList.Add(KeyDataEntry.ConstructInt("HG", cb.hunger));
            dataList.dataList.Add(KeyDataEntry.ConstructInt("EN", cb.energy));
            dataList.dataList.Add(KeyDataEntry.ConstructInt("MN", cb.mana));

            var entity = target.GetComponent<DGEntity>();
            dataList.dataList.Add(KeyDataEntry.ConstructInt("FX", entity.FaceDir.x));
            dataList.dataList.Add(KeyDataEntry.ConstructInt("FZ", entity.FaceDir.z));
        }
        return dataList;
    }
}
