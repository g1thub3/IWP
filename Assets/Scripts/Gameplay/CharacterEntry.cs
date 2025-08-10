using System.Text;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.TextCore.Text;
using System.Runtime.InteropServices.WindowsRuntime;

[System.Serializable]
public class CharacterStat {
    public int baseStat = 5;
    public int incrementPercentage = 15;
    private int currStat = 5;

    public int CurrStat
    {
        get { return currStat; }
        set { currStat = value; }
    }

    public void Apply(CharacterStat other)
    {
        baseStat = other.baseStat;
        incrementPercentage = other.incrementPercentage;
    }

    public void CalculateAfterLevel(int level)
    {
        currStat = baseStat + (int)Mathf.Floor((baseStat * ((float)(incrementPercentage * level - 1) / 100)));
    }
}


[System.Serializable]
public class CharacterEntry
{
    private static int ID_inc = 0;
    public int ID;
    public CHARACTER_ENUM associatedCharacter; // These are their actual stats
    public CharacterProfile Profile
    {
        get { return CharacterProfiles.Instance.characterProfiles[(int)associatedCharacter]; }
    }

    public string characterName; // note: use this more often

    public CharacterStat maxHealth;
    public CharacterStat hungerSize;

    public CharacterStat physAtk;
    public CharacterStat physDef;
    public CharacterStat magicAtk;
    public CharacterStat magicDef;

    public CharacterStat maxEnergy;
    public CharacterStat maxMana;

    public int experienceAward;

    public int viewDistance = 5;

    public int experiencePoints;
    public int characterLevel;
    public static readonly int maxLevel = 100;

    private Item heldItem;

    public int ExpToNextLevel
    {
        get
        {
            if (characterLevel == maxLevel) return 0;
            return characterLevel * 100;
        }
    }
    public int ExperienceAward
    {
        get
        {
            return experienceAward * characterLevel;
        }
    }

    public int GainXP(int amount) // Returns level difference
    {
        if (characterLevel == maxLevel) return 0;

        int lvlsIncreaased = 0;
        experiencePoints += amount;
        while (characterLevel < maxLevel && experiencePoints >= ExpToNextLevel)
        {
            experiencePoints -= ExpToNextLevel;
            characterLevel++;
            lvlsIncreaased++;
            if (characterLevel == maxLevel)
            {
                experiencePoints = 0;
            }
        }
        return lvlsIncreaased;
    }

    public string GetDescription()
    {
        StringBuilder description = new StringBuilder();
        description.AppendLine("Level: " + characterLevel + " | EXP: " + experiencePoints + " / " + ExpToNextLevel);
        if (HeldItem == null)
        {
            description.AppendLine("Held Item: None");
        } else
        {
            description.AppendLine("Held Item: " + HeldItem.ToString());
        }
        description.Append("\n");
        description.AppendLine("HP: " + maxHealth.CurrStat + " | Hunger: " + hungerSize.CurrStat);
        description.AppendLine("Energy: " + maxEnergy.CurrStat + " | Mana: " + maxMana.CurrStat);
        description.Append("\n");
        description.AppendLine("PA: " + physAtk.CurrStat + " | PD: " + physDef.CurrStat);
        description.AppendLine("MA: " + magicAtk.CurrStat + " | MD: " + magicDef.CurrStat);

        return description.ToString();
    }

    public Item HeldItem
    {
        get { return heldItem; }
        set {
            if (heldItem != null)
            {
                if (heldItem.module != null)
                    heldItem.module.RemoveEffect(this);
            }
            heldItem = value;
            if (heldItem != null)
            {
                if (heldItem.module != null)
                    heldItem.module.ApplyEffect(this);
            }
        }
    }

    public CharacterEntry()
    {
        maxHealth = new CharacterStat();
        hungerSize = new CharacterStat();
        physAtk = new CharacterStat();
        physDef = new CharacterStat();
        magicAtk = new CharacterStat();
        magicDef = new CharacterStat();
        maxEnergy = new CharacterStat();
        maxMana = new CharacterStat();
        heldItem = null;
    }

    public void ApplyCharacter()
    {
        if (associatedCharacter != CHARACTER_ENUM.NUM_CHARACTERS)
        {
            CharacterProfile foundCharacter = CharacterProfiles.Instance.characterProfiles[(int)associatedCharacter];
            maxHealth.Apply(foundCharacter.maxHealth);
            hungerSize.Apply(foundCharacter.hungerSize);
            physAtk.Apply(foundCharacter.physAtk);
            physDef.Apply(foundCharacter.physDef);
            magicAtk.Apply(foundCharacter.magicAtk);
            magicDef.Apply(foundCharacter.magicDef);
            maxEnergy.Apply(foundCharacter.maxEnergy);
            maxMana.Apply(foundCharacter.maxMana);
            experienceAward = foundCharacter.expAward;
        }
    }
    public void Recalculate()
    {
        if (heldItem != null)
            if (heldItem.module != null)
                heldItem.module.RemoveEffect(this);

        maxHealth.CalculateAfterLevel(characterLevel);
        hungerSize.CalculateAfterLevel(characterLevel);
        physAtk.CalculateAfterLevel(characterLevel);
        physDef.CalculateAfterLevel(characterLevel);
        magicAtk.CalculateAfterLevel(characterLevel);
        magicDef.CalculateAfterLevel(characterLevel);
        maxEnergy.CalculateAfterLevel(characterLevel);
        maxMana.CalculateAfterLevel(characterLevel);

        if (heldItem != null)
            if (heldItem.module != null)
                heldItem.module.ApplyEffect(this);
    }

    public static CharacterEntry Create(CHARACTER_ENUM chosenCharacter, int startingLevel, int givenID = -1)
    {
        CharacterEntry newChar = new CharacterEntry();
        newChar.associatedCharacter = chosenCharacter;
        newChar.characterName = newChar.Profile.characterName;
        newChar.characterLevel = startingLevel;
        newChar.viewDistance = 5;
        newChar.ApplyCharacter();
        newChar.Recalculate();
        if (givenID == -1)
        {
            ID_inc++;
            newChar.ID = ID_inc;
        } else
        {
            newChar.ID = givenID;
            if (ID_inc < givenID)
            {
                ID_inc = givenID + 1;
            }
        }
        return newChar;
    }
}
