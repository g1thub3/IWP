using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterSaveData
{
    public int charEnum;
    public int exp;
    public int level;
    public string heldItemKey;
}

[System.Serializable]
public class QuestSaveData
{
    public string clientName;
    public int clientCharEnum;
    public int compLevel;
    public bool isActive;

    public int questType;
    public string dungeon;
    public int floor;
    public bool questPossible;
    public bool questCompleted;
}

[System.Serializable]
public class DungeonTileSaveData
{
    public bool isWall;
    public string itemKey;
    public bool hasStairs;
}

[System.Serializable]
public class DungeonRoomSaveData
{
    public int positionX, positionY;
    public int length, height, padding;
}

[System.Serializable]
public class DungeonEntitySaveData
{
    public int positionX, positionY;
    public string direction;

    public bool isPlayer;
    public int charEnum;
    public int alliance;
    public int health;
    public int hunger;
    public int energy;
    public int mana;
    public string heldItemKey;
}

[System.Serializable]
public class DungeonSaveData
{
    public int floorNumber;
    public int currentTurn;
    public string floorName;

    public DungeonTileSaveData[] tiles;
    public List<DungeonRoomSaveData> rooms;

    public List<DungeonEntitySaveData> activeParty;
    public List<DungeonEntitySaveData> activeEntities;
}

[System.Serializable]
public class StorySaveData
{
    public bool isCompleted;
    public string storyName;
    public int currentState;
    public KeyDataList specialData;
    public string targetDungeon;
}

[System.Serializable]
public class GameSaveData
{
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
    public List<QuestSaveData> quests;
    public List<string> availableDungeons;

    public List<string> completedStories;
    public List<StorySaveData> activeStories;

    public DungeonSaveData dungeonSave;
}


[CreateAssetMenu(fileName = "SaveDataManager", menuName = "Scriptable Objects/SaveDataManager")]
public class SaveDataManager : SingletonScriptableObject<SaveDataManager>
{
    private GameSaveData saveFile;
    private GameSaveData quicksaveFile;
}
