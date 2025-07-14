using System.Collections.Generic;
using UnityEngine;

public enum CHARACTER_ENUM
{
    CHERRY,
    DAMSON,
    BANDIT,
    CAVE_THIEF,
    ADVENTURER_KNIGHT,
    ADVENTURER_MAGE,
    ADVENTURER_BRUTE,
    ADVENTURER_FIGHTER,
    ADVENTURER_PSYCHIC,
    ARMOURY,
    BANKER,
    MERCHANT,
    WAREHOUSE,
    VILLAGER_1,
    VILLAGER_2,
    VILLAGER_3,
    VILLAGER_4,
    VILLAGER_5,
    VILLAGER_6,
    VILLAGER_7,
    VILLAGER_8,
    VILLAGER_9,
    VILLAGER_10,
    FERN,
    DAHLIA,
    NUM_CHARACTERS
}

[System.Serializable]
public class CharacterProfile
{
    [HideInInspector]
    public string name = "yup";

    [Header("Visual")]
    public Sprite characterSprite;
    public string characterName;
    public RuntimeAnimatorController animatorController;

    [Header("Gameplay")] // These are the base stats the characters need to follow
    public CharacterStat maxHealth;
    public CharacterStat hungerSize;

    public CharacterStat physAtk;
    public CharacterStat physDef;
    public CharacterStat magicAtk;
    public CharacterStat magicDef;

    public CharacterStat maxEnergy;
    public CharacterStat maxMana;

    public int expAward;

    public List<CombatMove> availableMoves;
}

[CreateAssetMenu(fileName = "CharacterProfiles", menuName = "Scriptable Objects/CharacterProfiles")]
public class CharacterProfiles : SingletonScriptableObject<CharacterProfiles>
{
    public CharacterProfile[] characterProfiles = new CharacterProfile[(int)CHARACTER_ENUM.NUM_CHARACTERS];
    public List<string> questNPCNames = new List<string>();
    public List<CHARACTER_ENUM> possibleCompetitors;
    public CHARACTER_ENUM GetRandomEnum()
    {
        return (CHARACTER_ENUM)Random.Range(0, (int)CHARACTER_ENUM.NUM_CHARACTERS);
    }
    public string GetRandomNPCName()
    {
        return questNPCNames[Random.Range(0, questNPCNames.Count)];
    }
}