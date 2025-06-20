using UnityEngine;

public enum COMBAT_STAT { 
    PHYSICAL_ATK,
    PHYSICAL_DEF,
    MAGIC_ATK,
    MAGIC_DEF
}

[CreateAssetMenu(fileName = "StatChanger", menuName = "Items/StatChanger")]
public class StatChanger : ItemData
{
    public int amount;
    public COMBAT_STAT toChange;

    public override void ApplyEffect(CharacterEntry holder)
    {
        switch (toChange) { 
            case COMBAT_STAT.PHYSICAL_ATK:
                holder.physAtk.CurrStat += amount;
                break;
            case COMBAT_STAT.PHYSICAL_DEF:
                holder.physDef.CurrStat += amount;
                break;
            case COMBAT_STAT.MAGIC_ATK:
                holder.magicAtk.CurrStat += amount;
                break;
            case COMBAT_STAT.MAGIC_DEF:
                holder.magicDef.CurrStat += amount;
                break;
        }
    }
    public override void RemoveEffect(CharacterEntry holder)
    {
        switch (toChange)
        {
            case COMBAT_STAT.PHYSICAL_ATK:
                holder.physAtk.CurrStat -= amount;
                break;
            case COMBAT_STAT.PHYSICAL_DEF:
                holder.physDef.CurrStat -= amount;
                break;
            case COMBAT_STAT.MAGIC_ATK:
                holder.magicAtk.CurrStat -= amount;
                break;
            case COMBAT_STAT.MAGIC_DEF:
                holder.magicDef.CurrStat -= amount;
                break;
        }
    }
}
