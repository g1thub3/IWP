using System.Collections.Generic;
using UnityEngine;

public class QuestCompetitor
{
    public static int minProgress = 40;
    public static int maxProgress = 100;

    public Quest associatedQuest;
    public string competitorName;
    public int currentFloor;
    public int defaultProgress;
    public int floorProgress;
    public List<CharacterEntry> party;
    public List<DGEntity> partySpawned;
    public DGObject target = null;

    public void Progress()
    {
        floorProgress--;
        if (floorProgress <= 0)
        {
            defaultProgress = maxProgress;
            floorProgress = defaultProgress;
            currentFloor++;
        }
    }
}
