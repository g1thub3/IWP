using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public abstract class DGAIModule : SingletonScriptableObject<DGAIModule>
{
    public abstract void Run(DGNPC user, KeyDataList dataList = null);
}
