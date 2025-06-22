using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface DGAIModule
{
    public abstract void Run(DGNPC user, KeyDataList dataList = null);
}
