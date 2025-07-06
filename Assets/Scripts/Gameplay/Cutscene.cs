using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CutsceneSetup
{
    public GameObject props; // Obj to add to the scene for the cutscene to work with
    public List<string> toOmit; // Gameobjects already in the scene to be deactivated and reactivated once the scene is over
    GameObject cutsceneObjects;
    List<GameObject> omitted;

    public GameObject CutsceneObjects
    {
        get { return cutsceneObjects; }
    }
    public void AddProps()
    {
        if (props == null) return;
        cutsceneObjects = MonoBehaviour.Instantiate(props, Vector3.zero, Quaternion.identity);
    }
    public void Omit()
    {
        omitted = new List<GameObject>();
        for (int i = 0; i < toOmit.Count; i++) { 
            var obj = GameObject.Find(toOmit[i]);
            if (obj != null)
            {
                omitted.Add(obj);
                obj.SetActive(false);
            }
        }
    }
    public void UndoOmit()
    {
        foreach (var obj in omitted)
        {
            obj.SetActive(true);
        }
    }
}

[System.Serializable]
public class CutsceneInstruction
{
    public CUTSCENE_FUNCTION function;
    public bool Yield;
    public KeyDataList Data;
}

[CreateAssetMenu(fileName = "Cutscene", menuName = "Scriptable Objects/Cutscene")]
public class Cutscene : ScriptableObject
{
    [System.Serializable]
    public class CutsceneSetUpKey
    {
        public string key;
        public CutsceneSetup setup;
    }
    public List<CutsceneSetUpKey> setups;
    public List<CutsceneInstruction> instructions;
    public CutsceneSetup FindSetup(string key)
    {
        foreach (var data in setups) {
            if (data.key == key)
                return data.setup;
        }
        return null;
    }
}
