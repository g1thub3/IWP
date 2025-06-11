using UnityEngine;

public enum CHARACTER_STAT
{
    HEALTH,
    HUNGER,
    ENERGY,
    MANA
}

[CreateAssetMenu(fileName = "ReplenishingItem", menuName = "Items/ReplenishingItem")]
public class ReplenishingItem : ItemData
{
    public int amount;
    public CHARACTER_STAT toReplenish;
    public override void Use(CharacterBehaviour user)
    {
        user.Replenish(amount, toReplenish);
    }
}
