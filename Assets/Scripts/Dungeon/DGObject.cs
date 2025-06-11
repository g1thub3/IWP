using UnityEngine;

public abstract class DGObject : MonoBehaviour
{
    protected DungeonFloor floor;
    protected TileCoord position;
    protected void Start()
    {
        transform.localScale *= TileInfo.tileScale;
    }
    public void Set(DungeonFloor mfloor, TileCoord mposition)
    {
        floor = mfloor;
        position = mposition;
    }

    public DungeonFloor Floor { get { return floor; } }

    public TileCoord Position
    {
        get { return position; } 
        set {  position = value; }
    }
}
