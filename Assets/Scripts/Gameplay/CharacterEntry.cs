using System.Text;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.TextCore.Text;

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
        currStat = baseStat + (baseStat * ((incrementPercentage * level - 1) / 100));
    }
}


[System.Serializable]
public class CharacterEntry
{
    public CHARACTER_ENUM associatedCharacter; // These are their actual stats
    public CharacterProfile Profile
    {
        get { return CharacterProfiles.Instance.characterProfiles[(int)associatedCharacter]; }
    }

    public CharacterStat maxHealth;
    public CharacterStat hungerSize;

    public CharacterStat physAtk;
    public CharacterStat physDef;
    public CharacterStat magicAtk;
    public CharacterStat magicDef;

    public CharacterStat maxEnergy;
    public CharacterStat maxMana;

    public int experiencePoints;
    public int characterLevel;
    public static readonly int maxLevel = 100;

    private Item heldItem;

    public int ExpToNextLevel
    {
        get
        {
            return characterLevel * 100;
        }
    }

    public string GetDescription()
    {
        StringBuilder description = new StringBuilder();
        description.AppendLine("Level: " + characterLevel + " | EXP: " + experiencePoints + " / " + ExpToNextLevel);
        description.AppendLine("Held Item: " + HeldItem.ToString());
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

    public static CharacterEntry Create(CHARACTER_ENUM chosenCharacter, int startingLevel)
    {
        CharacterEntry newChar = new CharacterEntry();
        newChar.associatedCharacter = chosenCharacter;
        newChar.characterLevel = startingLevel;
        newChar.ApplyCharacter();
        newChar.Recalculate();
        return newChar;
    }
}
