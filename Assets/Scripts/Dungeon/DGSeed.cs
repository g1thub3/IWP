using UnityEngine;

public abstract class DGSeed : ScriptableObject
{
    public abstract DungeonFloor Generate(DGData dungeonData);
    public abstract void AddEnemies(DGGenerator dgGen);
    public abstract void AddItems(DGGenerator dgGen);
}