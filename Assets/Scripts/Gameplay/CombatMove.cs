using System.Text;
using Unity.VisualScripting;
using UnityEngine;

public enum MOVE_TYPE
{
    PHYSICAL,
    MAGICAL
}

public abstract class CombatMove : SingletonScriptableObject<CombatMove>
{
    public string moveName;
    public string moveDescription;
    public int energyRequirement;
    public CHARACTER_STAT consumptionType;

    public virtual bool CanBePerformed(CharacterBehaviour user) {
        switch (consumptionType)
        {
            case CHARACTER_STAT.MANA:
                return user.mana >= energyRequirement;
            default:
            case CHARACTER_STAT.ENERGY:
                return user.energy >= energyRequirement;
        }
    }
    public abstract bool WillMoveSucceed(CharacterBehaviour user);
    public virtual bool Perform(CharacterBehaviour user) {
        user.Consume(energyRequirement, consumptionType);
        return true;
    }

    public virtual string MoveDescription {
        get {  return moveDescription; }
    }
}

public abstract class AttackMove : CombatMove
{
    public int baseDamage;
    public MOVE_TYPE moveType;

    public override string MoveDescription
    {
        get
        {
            StringBuilder newString = new StringBuilder();
            newString.Append("Move: " + moveName + "\n");
            newString.Append(energyRequirement + " " + (consumptionType == CHARACTER_STAT.ENERGY ? "EN" : "MN") + " Needed\n");
            newString.Append("DMG: " + baseDamage + "\n");
            newString.Append("Type: " + (moveType == MOVE_TYPE.PHYSICAL ? "Physical" : "Magic"));
            newString.Append("\nDescription:\n");
            newString.Append(moveDescription);
            return newString.ToString();
        }
    }
}

//public class CombatMoveData {
//    public string moveKey;
//    public CombatMove module;
//    private static CombatMove[] foundAssets;

//    private static void FindAssets()
//    {
//        foundAssets = Resources.LoadAll<CombatMove>("");
//    }
//    public void Implement()
//    {
//        if (foundAssets == null)
//            FindAssets();
//        // Search for item in Resources
//        foreach (CombatMove move in foundAssets)
//        {
//            if (move.moveName.Equals(moveKey))
//            {
//                module = move;
//                break;
//            }
//        }
//    }
//    public void Set()
//    {
//        if (moveKey != null && module == null)
//        {
//            Implement();
//        }
//        else if (module != null && moveKey == null)
//        {
//            moveKey = module.moveName;
//        }
//    }
//}