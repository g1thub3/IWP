using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Quest;

[System.Serializable]
public class CharacterSaveData
{
    public string characterName;
    public int charEnum;
    public int exp;
    public int level;
    public string heldItemKey;

    public static CharacterSaveData Construct(CharacterEntry entry)
    {
        var newData = new CharacterSaveData();
        newData.characterName = entry.characterName;
        newData.charEnum = (int)entry.associatedCharacter;
        newData.exp = entry.experiencePoints;
        newData.level = entry.characterLevel;
        newData.heldItemKey = (entry.HeldItem != null ? entry.HeldItem.itemKey : string.Empty);
        return newData;
    }

    public CharacterEntry Extract()
    {
        var newChar = CharacterEntry.Create((CHARACTER_ENUM)charEnum, level);
        newChar.experiencePoints = exp;
        newChar.HeldItem = Item.New(heldItemKey);
        newChar.characterName = characterName;
        return newChar;
    }
}

[System.Serializable]
public class QuestSaveData
{
    public int questID;
    public string clientName;
    public int clientCharEnum;
    public int compLevel;
    public bool isActive;

    public int questType;
    public string dungeon;
    public int floor;
    public bool questPossible;
    public bool questCompleted;

    public KeyDataList dataList;

    public static QuestSaveData Construct(Quest quest)
    {
        var newData = new QuestSaveData();
        newData.questID = quest.questID;
        newData.clientName = quest.clientName;
        newData.clientCharEnum = (int)quest.clientCharacter;
        newData.compLevel = quest.competitiveLevel;
        newData.isActive = quest.isActive;

        newData.questType = (int)quest.QuestType;
        newData.dungeon = quest.quest.dungeon.dungeonName;
        newData.floor = quest.quest.floor;
        newData.questPossible = quest.quest.questPossible;
        newData.questCompleted = quest.quest.questCompleted;

        newData.dataList = quest.quest.GetData();

        return newData;
    }

    public Quest Extract()
    {
        QuestData qData;
        switch ((QUEST_TYPE)questType)
        {
            default:
            case QUEST_TYPE.RETRIEVAL:
                qData = new RetrievalQuest(dataList.GetData("Key").String);
                break;
            case QUEST_TYPE.RESCUE:
                qData = new RescueQuest(clientName, (CHARACTER_ENUM)clientCharEnum);
                break;
        }
        qData.dungeon = DGData.GetDungeon(dungeon);
        qData.floor = floor;
        qData.questPossible = questPossible;
        qData.questCompleted = questCompleted;
        qData.dataList = dataList;

        var newQuest = new Quest(qData, clientName, (CHARACTER_ENUM)clientCharEnum);
        newQuest.competitiveLevel = compLevel;
        newQuest.CalculateDifficulty();
        newQuest.isActive = isActive;
        newQuest.questID = questID;
        return newQuest;
    }
}

[System.Serializable]
public class DungeonTileSaveData
{
    public bool isWall;
    public string itemKey;
    public bool hasStairs;

    public static DungeonTileSaveData Construct(TileInfo tile)
    {
        var newData = new DungeonTileSaveData();
        newData.isWall = tile.isWall;
        newData.hasStairs = tile.structure != null;
        newData.itemKey = string.Empty;
        if (tile.item != null)
        {
            if (!tile.item.GetComponent<DGItemContainer>().Item.IsQuestTarget)
                newData.itemKey = tile.item.GetComponent<DGItemContainer>().Item.itemKey;
        }
        return newData;
    }
}

[System.Serializable]
public class DungeonRoomSaveData
{
    public TileCoord origin;
    public int length, height, padding;
    public static DungeonRoomSaveData Construct(FloorRoom room)
    {
        var newData = new DungeonRoomSaveData();
        newData.origin = room.origin;
        newData.length = room.length;
        newData.height = room.height;
        newData.padding = room.padding;
        return newData;
    }
}

[System.Serializable]
public class DungeonQCSaveData
{
    public string competitorName;
    public int currentFloor;
    public int defaultProgress;
    public int floorProgress;
    public List<CharacterSaveData> party;
    public List<DungeonEntitySaveData> partySpawned; // Note: Add it aligned with index

    public QuestCompetitor Extract(Quest associated)
    {
        var newComp = new QuestCompetitor();
        newComp.associatedQuest = associated;
        newComp.competitorName = competitorName;
        newComp.currentFloor = currentFloor;
        newComp.defaultProgress = defaultProgress;
        newComp.floorProgress = floorProgress;
        newComp.party = new List<CharacterEntry>();
        foreach (var member in party)
        {
            newComp.party.Add(member.Extract());
        }
        return newComp;
    }

    public static DungeonQCSaveData Construct(QuestCompetitor comp)
    {
        var newData = new DungeonQCSaveData();
        newData.competitorName = comp.competitorName;
        newData.currentFloor = comp.currentFloor;
        newData.defaultProgress = comp.defaultProgress;
        newData.floorProgress = comp.floorProgress;
        newData.party = new List<CharacterSaveData>();
        for (int i = 0; i < comp.party.Count; i++)
        {
            newData.party.Add(CharacterSaveData.Construct(comp.party[i]));
        }

        newData.partySpawned = new List<DungeonEntitySaveData>();
        for (int i = 0; i < comp.partySpawned.Count; i++)
        {
            newData.partySpawned.Add(DungeonEntitySaveData.Construct(comp.partySpawned[i]));
        }
        return newData;
    }
}

[System.Serializable]
public class DungeonQCSaveDataEntry
{
    public int associatedID;
    public List<DungeonQCSaveData> competitors;

    public static DungeonQCSaveDataEntry Construct(Quest q, List<QuestCompetitor> comp)
    {
        var newData = new DungeonQCSaveDataEntry();
        newData.associatedID = q.questID;

        newData.competitors = new List<DungeonQCSaveData>();
        for (int i = 0; i < comp.Count; i++)
        {
            newData.competitors.Add(DungeonQCSaveData.Construct(comp[i]));
        }

        return newData;
    }
}

[System.Serializable]
public class DungeonEntitySaveData
{
    public TileCoord position;
    public TileCoord direction;

    public int health;
    public int hunger;
    public int energy;
    public int mana;

    public CharacterSaveData character;

    public void LoadStats(CharacterBehaviour cb)
    {
        cb.health = health;
        cb.hunger = hunger;
        cb.energy = energy;
        cb.mana = mana;
    }

    public static DungeonEntitySaveData Construct(DGEntity entity)
    {
        var newData = new DungeonEntitySaveData();
        newData.position = entity.Position;
        newData.direction = entity.FaceDir;

        var cb = entity.GetComponent<CharacterBehaviour>();
        newData.health = cb.health;
        newData.hunger = cb.hunger;
        newData.energy = cb.energy;
        newData.mana = cb.mana;

        newData.character = CharacterSaveData.Construct(cb.character);
        return newData;
    }
}

public class DungeonItemSaveData // EXCLUDE QUEST TARGETS
{
    public TileCoord position;
    public string itemKey;
}

[System.Serializable]
public class DungeonFloorSaveData
{
    public DungeonTileSaveData[] tiles;
    public List<DungeonRoomSaveData> rooms;
    private List<DungeonItemSaveData> items;
    public List<DungeonEntitySaveData> activeParty;
    public List<CharacterSaveData> tempParty;
    public List<DungeonEntitySaveData> activeEnemies; // EXCLUDE QUEST TARGETS

    public DungeonFloor ExtractFloor()
    {
        items = new List<DungeonItemSaveData>();

        DungeonFloor floorData = new DungeonFloor();
        floorData.Fill();
        floorData.nonWallTiles = new List<TileInfo>();

        for (int i = 0; i < tiles.Length; i++)
        {
            floorData.tiles[i].isWall = tiles[i].isWall;
            if (!floorData.tiles[i].isWall)
            {
                floorData.nonWallTiles.Add(floorData.tiles[i]);
            }
            if (tiles[i].hasStairs)
            {
                floorData.tiles[i].AddStructure(Tilesets.Instance.structureList.GetData("BasementStairs").Obj.GetComponent<DGInteractable>(), DINextFloor.Instance);
                floorData.stairs = floorData.tiles[i].structure;
            }
            if (!tiles[i].itemKey.Equals(string.Empty))
            {
                var newItem = new DungeonItemSaveData();
                newItem.position = floorData.IndexToCoord(i);
                newItem.itemKey = tiles[i].itemKey;
                items.Add(newItem);
            }
        }

        List<FloorRoom> floorRooms = new List<FloorRoom>();
        for (int i = 0; i < rooms.Count; i++)
        {
            var newRoom = new FloorRoom();
            newRoom.origin = rooms[i].origin;
            newRoom.length = rooms[i].length;
            newRoom.height = rooms[i].height;
            newRoom.padding = rooms[i].padding;
            floorRooms.Add(newRoom);
        }
        floorData.rooms = floorRooms;
        return floorData;
    }

    public void AddItems(DGGenerator dungeonGen)
    {
        if (items == null) return;
        foreach (var item in items)
        {
            dungeonGen.InsertItem(Tilesets.Instance.ConstructItemInteractable(Item.New(item.itemKey)), dungeonGen.CurrentFloor.CoordToTileInfo(item.position));
        }
    }

    public void AddEnemies(DGGenerator dungeonGen)
    {
        foreach (var enemy in activeEnemies)
        {
            TileCoord point = enemy.position;
            CharacterEntry newCharacter = enemy.character.Extract();
            var newEnemy = dungeonGen.SpawnNPC(DG_CHARACTER_TYPE.ENEMY, newCharacter, point);
            enemy.LoadStats(newEnemy.GetComponent<CharacterBehaviour>());
            newEnemy.GetComponent<DGEntity>().FaceDir = enemy.direction;
            newEnemy.name = newCharacter.characterName;
        }
    }

    public static DungeonFloorSaveData Construct(DGGenerator generator)
    {
        var newData = new DungeonFloorSaveData();
        newData.tiles = new DungeonTileSaveData[generator.CurrentFloor.tiles.Length];
        for (int i = 0; i < generator.CurrentFloor.tiles.Length; i++)
        {
            newData.tiles[i] = DungeonTileSaveData.Construct(generator.CurrentFloor.tiles[i]);
        }

        newData.rooms = new List<DungeonRoomSaveData>();
        for (int i = 0; i < generator.CurrentFloor.rooms.Count; i++)
        {
            newData.rooms.Add(DungeonRoomSaveData.Construct(generator.CurrentFloor.rooms[i]));
        }

        newData.activeParty = new List<DungeonEntitySaveData>();
        for (int i = 0; i < generator.ActiveParty.Count; i++)
        {
            newData.activeParty.Add(DungeonEntitySaveData.Construct(generator.ActiveParty[i].GetComponent<DGEntity>()));
        }

        newData.tempParty = new List<CharacterSaveData>();
        for (int i = 0; i < generator.TempParty.Count; i++)
        {
            newData.tempParty.Add(CharacterSaveData.Construct(generator.TempParty[i]));
        }

        newData.activeEnemies = new List<DungeonEntitySaveData>();
        for (int i = 0; i < generator.ActiveEntities.Count; i++)
        {
            var cb = generator.ActiveEntities[i].GetComponent<CharacterBehaviour>();
            if (cb.alliance == 1)
            {
                newData.activeEnemies.Add(DungeonEntitySaveData.Construct(generator.ActiveEntities[i]));
            }
        }

        return newData;
    }
}

[System.Serializable]
public class DungeonSaveData
{
    public GameSaveData baseFile;

    public int floorNumber;
    public int currentTurn;
    public string floorName;

    public DungeonFloorSaveData floorData;
    public List<DungeonQCSaveDataEntry> questCompetitors;

    public static DungeonSaveData Construct(DGGameManager gameManager, DGGenerator generator)
    {
        var newData = new DungeonSaveData();
        newData.baseFile = GameSaveData.Construct();
        newData.floorNumber = gameManager.CurrentFloor;
        newData.currentTurn = gameManager.currentTurn;
        newData.floorName = gameManager.floorName;

        newData.floorData = DungeonFloorSaveData.Construct(generator);

        newData.questCompetitors = new List<DungeonQCSaveDataEntry>();
        foreach (var q in gameManager.ActiveQuests)
        {
            if (gameManager.QuestCompetitors.ContainsKey(q))
            {
                newData.questCompetitors.Add(DungeonQCSaveDataEntry.Construct(q, gameManager.QuestCompetitors[q]));
            }
        }

        return newData;
        //var newData = new DungeonSaveData();
        //return newData;
    }

    // When generating, take ownedQuests and set up from there
}

[System.Serializable]
public class StorySaveData
{
    public bool isCompleted;
    public string storyName;
    public int currentState;
    public KeyDataList specialData;
    public string targetDungeon;

    public static StorySaveData Construct(StoryData data)
    {
        var newData = new StorySaveData();
        newData.isCompleted = data.isCompleted;
        newData.storyName = data.storyName;
        newData.currentState = data.currentState;
        newData.specialData = data.specialData;
        newData.targetDungeon = data.targetDungeon != null ? data.targetDungeon.dungeonName : string.Empty;
        return newData;
    }

    public StoryData Extract()
    {
        var story = new StoryData(storyName);
        story.isCompleted = isCompleted;
        story.currentState = currentState;
        story.specialData = specialData;
        story.targetDungeon = DGData.GetDungeon(targetDungeon);
        return story;
    }
}

[System.Serializable]
public class GameSaveData
{
    public int playTime;
    public string sceneName;
    public string locationName;
    public string selectedDungeon;

    public int adventurerRanking;
    public int adventurerEXP;
    public int walletGold;
    public int bankGold;
    public List<CharacterSaveData> party;
    public List<string> inventoryItemKeys;
    public List<string> storageItemKeys;
    public List<string> merchantShopKeys;
    public List<string> armouryShopKeys;
    public List<QuestSaveData> ownedQuests;
    public List<QuestSaveData> availableQuests;
    public List<QuestSaveData> availableCompetitiveQuests;

    public List<string> availableDungeons;

    public List<string> completedStories;
    public List<StorySaveData> activeStories;

    public static GameSaveData Construct()
    {
        var newData = new GameSaveData();
        newData.playTime = (int)Mathf.Floor(GlobalGameManager.Instance.playTime);
        newData.sceneName = SceneManager.GetActiveScene().name;
        newData.locationName = GameSceneManager.Instance.locationName;
        newData.selectedDungeon = (GlobalGameManager.Instance.selectedDungeon != null ? GlobalGameManager.Instance.selectedDungeon.dungeonName : string.Empty);

        newData.adventurerRanking = GlobalGameManager.Instance.adventurerRanking;
        newData.adventurerEXP = GlobalGameManager.Instance.adventurerEXP;
        newData.walletGold = GlobalGameManager.Instance.ownedGold;
        newData.bankGold = GlobalGameManager.Instance.bankGold;

        newData.party = new List<CharacterSaveData>(GlobalGameManager.Instance.party.Count);
        for (int i = 0; i < GlobalGameManager.Instance.party.Count; i++)
        {
            newData.party.Add(CharacterSaveData.Construct(GlobalGameManager.Instance.party[i]));
        }

        newData.inventoryItemKeys = new List<string>(GlobalGameManager.Instance.inventory.Count);
        for (int i = 0; i < GlobalGameManager.Instance.inventory.Count; i++)
        {
            newData.inventoryItemKeys.Add(GlobalGameManager.Instance.inventory[i].itemKey);
        }

        newData.storageItemKeys = new List<string>(GlobalGameManager.Instance.storage.Count);
        for (int i = 0; i < GlobalGameManager.Instance.storage.Count; i++)
        {
            newData.storageItemKeys.Add(GlobalGameManager.Instance.storage[i].itemKey);
        }

        newData.merchantShopKeys = new List<string>(GlobalGameManager.Instance.merchantShop.Count);
        for (int i = 0; i < GlobalGameManager.Instance.merchantShop.Count; i++)
        {
            newData.merchantShopKeys.Add(GlobalGameManager.Instance.merchantShop[i].itemKey);
        }

        newData.armouryShopKeys = new List<string>(GlobalGameManager.Instance.armouryShop.Count);
        for (int i = 0; i < GlobalGameManager.Instance.armouryShop.Count; i++)
        {
            newData.armouryShopKeys.Add(GlobalGameManager.Instance.armouryShop[i].itemKey);
        }

        newData.availableDungeons = new List<string>(GlobalGameManager.Instance.availableDungeons.Count);
        for (int i = 0; i < GlobalGameManager.Instance.availableDungeons.Count; i++)
        {
            newData.availableDungeons.Add(GlobalGameManager.Instance.availableDungeons[i].dungeonName);
        }

        newData.completedStories = new List<string>(GameStoryManager.Instance.CompletedStories.Count);
        for (int i = 0; i < GameStoryManager.Instance.CompletedStories.Count; i++)
        {
            newData.completedStories.Add(GameStoryManager.Instance.CompletedStories[i]);
        }

        newData.activeStories = new List<StorySaveData>(GameStoryManager.Instance.ActiveStories.Count);
        for (int i = 0; i < GameStoryManager.Instance.ActiveStories.Count; i++)
        {
            newData.activeStories.Add(StorySaveData.Construct(GameStoryManager.Instance.ActiveStories[i]));
        }

        newData.ownedQuests = new List<QuestSaveData>(GlobalGameManager.Instance.ownedQuests.Count);
        for (int i = 0; i < GlobalGameManager.Instance.ownedQuests.Count; i++)
        {
            newData.ownedQuests.Add(QuestSaveData.Construct(GlobalGameManager.Instance.ownedQuests[i]));
        }

        newData.availableQuests = new List<QuestSaveData>(GlobalGameManager.Instance.availableQuests.Count);
        for (int i = 0; i < GlobalGameManager.Instance.availableQuests.Count; i++)
        {
            newData.availableQuests.Add(QuestSaveData.Construct(GlobalGameManager.Instance.availableQuests[i]));
        }

        newData.availableCompetitiveQuests = new List<QuestSaveData>(GlobalGameManager.Instance.availableCompetitiveQuests.Count);
        for (int i = 0; i < GlobalGameManager.Instance.availableCompetitiveQuests.Count; i++)
        {
            newData.availableCompetitiveQuests.Add(QuestSaveData.Construct(GlobalGameManager.Instance.availableCompetitiveQuests[i]));
        }
        return newData;
    }

    public void LoadGameSave()
    {
        GlobalGameManager.Instance.selectedDungeon = DGData.GetDungeon(selectedDungeon);
        GlobalGameManager.Instance.playTime = playTime;
        GlobalGameManager.Instance.adventurerRanking = adventurerRanking;
        GlobalGameManager.Instance.adventurerEXP = adventurerEXP;
        GlobalGameManager.Instance.ownedGold = walletGold;
        GlobalGameManager.Instance.bankGold = bankGold;
        GlobalGameManager.Instance.SetStorageLimit();

        GlobalGameManager.Instance.party = new List<CharacterEntry>(party.Count);
        for (int i = 0; i < party.Count; i++)
        {
            GlobalGameManager.Instance.party.Add(party[i].Extract());
        }

        GlobalGameManager.Instance.inventory = new List<Item>(inventoryItemKeys.Count);
        for (int i = 0; i < inventoryItemKeys.Count; i++)
        {
            GlobalGameManager.Instance.inventory.Add(Item.New(inventoryItemKeys[i]));
        }
        GlobalGameManager.Instance.storage = new List<Item>(storageItemKeys.Count);
        for (int i = 0; i < storageItemKeys.Count; i++)
        {
            GlobalGameManager.Instance.storage.Add(Item.New(storageItemKeys[i]));
        }
        GlobalGameManager.Instance.merchantShop = new List<Item>(merchantShopKeys.Count);
        for (int i = 0; i < merchantShopKeys.Count; i++)
        {
            GlobalGameManager.Instance.merchantShop.Add(Item.New(merchantShopKeys[i]));
        }
        GlobalGameManager.Instance.armouryShop = new List<Item>(armouryShopKeys.Count);
        for (int i = 0; i < armouryShopKeys.Count; i++)
        {
            GlobalGameManager.Instance.armouryShop.Add(Item.New(armouryShopKeys[i]));
        }

        GlobalGameManager.Instance.availableDungeons = new List<DGData>(availableDungeons.Count);
        for (int i = 0; i < availableDungeons.Count; i++)
        {
            GlobalGameManager.Instance.availableDungeons.Add(DGData.GetDungeon(availableDungeons[i]));
        }

        GameStoryManager.Instance.CompletedStories = new List<string>(completedStories.Count);
        for (int i = 0; i < completedStories.Count; i++)
        {
            GameStoryManager.Instance.CompletedStories.Add(completedStories[i]);
        }

        GameStoryManager.Instance.ActiveStories = new List<StoryData>(activeStories.Count);
        for (int i = 0; i < activeStories.Count; i++)
        {
            GameStoryManager.Instance.ActiveStories.Add(activeStories[i].Extract());
        }

        GlobalGameManager.Instance.ownedQuests = new List<Quest>(ownedQuests.Count);
        for (int i = 0; i < ownedQuests.Count; i++)
        {
            GlobalGameManager.Instance.ownedQuests.Add(ownedQuests[i].Extract());
        }
        GlobalGameManager.Instance.availableQuests = new List<Quest>(availableQuests.Count);
        for (int i = 0; i < availableQuests.Count; i++)
        {
            GlobalGameManager.Instance.availableQuests.Add(availableQuests[i].Extract());
        }
        GlobalGameManager.Instance.availableCompetitiveQuests = new List<Quest>(availableCompetitiveQuests.Count);
        for (int i = 0; i < availableCompetitiveQuests.Count; i++)
        {
            GlobalGameManager.Instance.availableCompetitiveQuests.Add(availableCompetitiveQuests[i].Extract());
        }
    }
}


[CreateAssetMenu(fileName = "SaveDataManager", menuName = "Scriptable Objects/SaveDataManager")]
public class SaveDataManager : SingletonScriptableObject<SaveDataManager>
{
    private string saveFilePath, quicksaveFilePath;
    [SerializeField] string savefileName = "BaseSaveFile";
    [SerializeField] string quicksavefileName = "QuickSaveFile";
    private GameSaveData _saveFile;
    private DungeonSaveData _quicksaveFile;

    public GameSaveData BaseFile
    {
        get { return _saveFile; }
    }
    public DungeonSaveData QuickSaveFile
    {
        get { return _quicksaveFile; }
    }

    public void WriteSaveData()
    {
        _saveFile = GameSaveData.Construct();
        string jsonSave = JsonUtility.ToJson(_saveFile);
        File.WriteAllText(saveFilePath, jsonSave);
    }
    public void ReadSaveData()
    {
        if (File.Exists(saveFilePath)) { 
            string filetext = File.ReadAllText(saveFilePath);
            _saveFile = JsonUtility.FromJson<GameSaveData>(filetext);
        } else
        {
            _saveFile = null;
        }
    }

    public void WriteQuicksaveData(DGGameManager gameManager, DGGenerator generator)
    {
        _quicksaveFile = DungeonSaveData.Construct(gameManager, generator);
        string jsonSave = JsonUtility.ToJson(_quicksaveFile);
        File.WriteAllText(quicksaveFilePath, jsonSave);
    }
    public void ReadQuicksaveData()
    {
        if (File.Exists(quicksaveFilePath))
        {
            string filetext = File.ReadAllText(quicksaveFilePath);
            _quicksaveFile = JsonUtility.FromJson<DungeonSaveData>(filetext);
        } else
        {
            _quicksaveFile = null;
        }
    }

    public void DestroyQuicksave()
    {
        if (File.Exists(quicksaveFilePath))
        {
            File.Delete(quicksaveFilePath);
        }
        _quicksaveFile = null;
    }

    private void OnEnable()
    {
        saveFilePath = string.Format("{0}/{1}.json", Application.persistentDataPath, savefileName);
        quicksaveFilePath = string.Format("{0}/{1}.json", Application.persistentDataPath, quicksavefileName);

        ReadSaveData();
        ReadQuicksaveData();
    }
}
