using UnityEngine;

public abstract class CombatMove : SingletonScriptableObject<CombatMove>
{
    public string moveName;
    public string moveDescription;
    public abstract bool Perform(CharacterBehaviour user);
}

public class CombatMoveData {
    public string moveKey;
    public CombatMove module;
    private static CombatMove[] foundAssets;

    private static void FindAssets()
    {
        foundAssets = Resources.LoadAll<CombatMove>("");
    }
    public void Implement()
    {
        if (foundAssets == null)
            FindAssets();
        // Search for item in Resources
        foreach (CombatMove move in foundAssets)
        {
            if (move.moveName.Equals(moveKey))
            {
                module = move;
                break;
            }
        }
    }
    public void Set()
    {
        if (moveKey != null && module == null)
        {
            Implement();
        }
        else if (module != null && moveKey == null)
        {
            moveKey = module.moveName;
        }
    }
}