using UnityEngine;

[CreateAssetMenu(fileName = "ViewDistanceItem", menuName = "Items/ViewDistanceItem")]
public class ViewDistanceItem : ItemData
{
    public int amount;
    public override void ApplyEffect(CharacterEntry holder)
    {
        holder.viewDistance += amount;
    }
    public override void RemoveEffect(CharacterEntry holder)
    {
        holder.viewDistance -= amount;
    }
}
