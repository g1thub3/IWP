using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CutsceneSetup
{
    public Transform CameraFocusPoint;
    public GameObject props; // Obj to add to the scene for the cutscene to work with
    public List<string> toOmit; // Gameobjects already in the scene to be deactivated and reactivated once the scene is over
    GameObject cutsceneObjects;
    List<GameObject> omitted;

    public Dictionary<string, CutsceneActor> Actors;
    public Dictionary<string, Transform> Points;
    public Dictionary<string, GameObject> CutsceneProps;
    public Transform Controller;

    public GameObject CutsceneObjects
    {
        get { return cutsceneObjects; }
    }
    public void SetUp()
    {
        Omit();
        Actors = new Dictionary<string, CutsceneActor>();
        Points = new Dictionary<string, Transform>();
        CutsceneProps = new Dictionary<string, GameObject>();
        if (props != null)
        {
            cutsceneObjects = MonoBehaviour.Instantiate(props, Vector3.zero, Quaternion.identity);
        } else
        {
            cutsceneObjects = new GameObject();
        }
        var focus = new GameObject();
        CameraFocusPoint = focus.transform;
        CameraFocusPoint.SetParent(cutsceneObjects.transform);

        Transform actors = cutsceneObjects.transform.Find("Actors");
        if (actors != null)
        {
            for (int i = 0; i < actors.childCount; i++)
            {
                if (actors.GetChild(i).TryGetComponent<CutsceneActor>(out CutsceneActor actor))
                    Actors.Add(actors.GetChild(i).gameObject.name, actor);
            }
        }

        Transform points = cutsceneObjects.transform.Find("Points");
        if (points != null)
        {
            for (int i = 0; i < points.childCount; i++)
            {
                Points.Add(points.GetChild(i).gameObject.name, points.GetChild(i));
            }
        }
        Transform cutprops = cutsceneObjects.transform.Find("CutsceneProps");
        if (cutprops != null)
        {
            for (int i = 0; i < cutprops.childCount; i++)
            {
                CutsceneProps.Add(cutprops.GetChild(i).gameObject.name, cutprops.GetChild(i).gameObject);
            }
        }
    }
    public void Omit()
    {
        omitted = new List<GameObject>();
        var dayComplete = GameObject.FindFirstObjectByType<DayCompleter>();
        if (dayComplete != null) {
            omitted.Add(dayComplete.gameObject);
            dayComplete.gameObject.SetActive(false);
        }
        var areaManager = GameObject.FindFirstObjectByType<FRAreaManager>();
        if (areaManager != null) { 
            omitted.Add(areaManager.gameObject);
            areaManager.gameObject.SetActive(false);
        }
        var controller = GameObject.FindFirstObjectByType<FRController>();
        if (controller != null)
        {
            Controller = controller.transform;
            omitted.Add(controller.gameObject);
            controller.gameObject.SetActive(false);
        }
        var gameManager = GameObject.FindFirstObjectByType<DGGameManager>();
        if (gameManager != null)
        {
            omitted.Add(gameManager.gameObject);
            gameManager.gameObject.SetActive(false);
        }
        var ui = GameObject.FindFirstObjectByType<DungeonUIHandler>();
        if (ui != null)
        {
            omitted.Add(ui.gameObject);
            ui.gameObject.SetActive(false);
        }
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
            if (obj.GetComponent<FRController>() != null)
            {
                CutsceneManager.Instance.currentCinemachine.Follow = obj.transform;
            }
        }
    }
}

[System.Serializable]
public class CutsceneInstruction
{
    [HideInInspector] public string name;
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
