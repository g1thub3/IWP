using UnityEngine;

[System.Serializable]
public class NPCGenData
{
    public CHARACTER_ENUM character;
    public uint minLevel;
    public uint maxLevel;
}
public abstract class DGSeed : ScriptableObject
{
    public abstract DungeonFloor Generate(DGData dungeonData);
    public abstract void AddEnemies(DGGenerator dgGen);
}