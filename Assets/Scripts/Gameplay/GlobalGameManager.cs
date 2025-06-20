using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "GlobalGameManager", menuName = "Scriptable Objects/GlobalGameManager")]
public class GlobalGameManager : SingletonScriptableObject<GlobalGameManager>
{
    [Header("Game Data")]
    public static int adventurerRankMax = 30;
    public int adventurerEXP;
    public int adventurerRanking;
    public int ownedGold;
    public void AddAdventurerEXP(int amt)
    {
        adventurerEXP += amt;
    }
    
    public static int partyLimit = 4;
    public List<CharacterEntry> party = new List<CharacterEntry>();

    public static int inventoryLimit = 20;
    public List<Item> inventory = new List<Item>();

    public void AddItem(Item newItem)
    {
        inventory.Add(newItem);
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
        for (int i = 0; i < maxQuestCount; i++)
        {
            var newQuest = Quest.CreateQuestData(Quest.QUEST_TYPE.RETRIEVAL);
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
            int compLevel = 1 + (Random.Range(0, adventurerRanking));
            var newQuest = Quest.CreateQuestData(Quest.QUEST_TYPE.RETRIEVAL, compLevel);
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

    private void OnEnable()
    {
        CycleDay();
        ownedQuests.Clear();
    }
}
