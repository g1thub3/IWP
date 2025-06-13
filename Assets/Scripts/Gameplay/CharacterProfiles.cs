using System.Collections.Generic;
using UnityEngine;

public enum CHARACTER_ENUM
{
    CHERRY,
    DAMSON,
    BANDIT,
    CAVE_THIEF,
    NUM_CHARACTERS,
}

[System.Serializable]
public class CharacterProfile
{
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

    public DGAIModule aiModule;
}

[CreateAssetMenu(fileName = "CharacterProfiles", menuName = "Scriptable Objects/CharacterProfiles")]
public class CharacterProfiles : SingletonScriptableObject<CharacterProfiles>
{
    public CharacterProfile[] characterProfiles = new CharacterProfile[(int)CHARACTER_ENUM.NUM_CHARACTERS];
    public List<string> questNPCNames = new List<string>();
    public string GetRandomNPCName()
    {
        return questNPCNames[Random.Range(0, questNPCNames.Count)];
    }
}