using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.Rendering.STP;



[CreateAssetMenu(fileName = "CutsceneManager", menuName = "Scriptable Objects/CutsceneManager")]
public class CutsceneManager : SingletonScriptableObject<CutsceneManager>, IYieldable
{
    private void OnEnable()
    {
        _isCutsceneRunning = false;
        CutsceneFunctions.Instantiate();
    }
    public bool IsInProgress()
    {
        return _isCutsceneRunning;
    }

    public void RunCutscene(Cutscene cutscene)
    {
        if (_isCutsceneRunning) return;
        CutsceneStart();
        GlobalCanvasManager.Instance.StartCoroutine(CutsceneCoroutine(cutscene));
    }

    public delegate bool YieldCheck();
    public YieldCheck FunctionYieldCheck;
    public CutsceneSetup CurrentSetup;
    private bool _isCutsceneRunning = false;
    public CinemachineCamera _currentCinemachine;

    private IEnumerator CutsceneCoroutine(Cutscene cutscene)
    {
        for (int i = 0; i < cutscene.instructions.Count; i++)
        {
            var instruction = cutscene.instructions[i];
            CutsceneFunctions.Run(instruction, cutscene);
            if (FunctionYieldCheck != null)
            {
                while (FunctionYieldCheck.Invoke())
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }
        CutsceneEnd();
    }


    public void CutsceneStart()
    {
        _isCutsceneRunning = true;
    }
    public void CutsceneEnd()
    {
        _isCutsceneRunning = false;
        CutsceneFunctions.CleanScene();
    }
    public CutsceneActor GetActor(string name)
    {
        if (CurrentSetup == null) return null;
        return CurrentSetup.Actors[name];
    }
    public Transform GetPoint(string name)
    {
        if (CurrentSetup == null) return null;
        return CurrentSetup.Points[name];
    }
    public GameObject GetProp( string name)
    {
        if (CurrentSetup == null) return null;
        return CurrentSetup.CutsceneProps[name];
    }
}


public enum CUTSCENE_FUNCTION
{
    SCENESETUP,
    DIALOGUE,
    ACTOR_FACE,
    ACTOR_MOVE,
    ACTOR_ANIMATE,
    SET_CONTROLLER_POSITION,
    SET_CAMERA_FOCUS,
    DESTROY_PROP
}

public static class CutsceneFunctions
{
    private static bool Yield;
    private static bool SceneSetupInProgress;

    public static void Instantiate()
    {
        Yield = false;
        functions = new Dictionary<CUTSCENE_FUNCTION, System.Action<Cutscene, KeyDataList>>();
        functions.Add(CUTSCENE_FUNCTION.SCENESETUP, SceneSetup);
        functions.Add(CUTSCENE_FUNCTION.DIALOGUE, Dialogue);
        functions.Add(CUTSCENE_FUNCTION.ACTOR_FACE, FaceActor);
        functions.Add(CUTSCENE_FUNCTION.ACTOR_MOVE, MoveActor);
        functions.Add(CUTSCENE_FUNCTION.ACTOR_ANIMATE, AnimateActor);
        functions.Add(CUTSCENE_FUNCTION.SET_CONTROLLER_POSITION, SetControllerPosition);
        functions.Add(CUTSCENE_FUNCTION.SET_CAMERA_FOCUS, SetCameraFocus);
        functions.Add(CUTSCENE_FUNCTION.DESTROY_PROP, DestroyProp);
    }
    public static Dictionary<CUTSCENE_FUNCTION, System.Action<Cutscene, KeyDataList>> functions;
    public static void Run(CutsceneInstruction instruction, Cutscene cutscene)
    {
        Yield = instruction.Yield;
        CutsceneManager.Instance.FunctionYieldCheck = null;
        functions[instruction.function].Invoke(cutscene, instruction.Data);
    }
    private static IEnumerator SetupCoroutine(Cutscene cutscene, KeyDataList dataList)
    {
        var key = dataList.GetData("SetupKey");
        CutsceneManager.Instance.CurrentSetup = cutscene.FindSetup(key.String);
        GameSceneManager.Instance.previousArea = SceneManager.GetActiveScene().name;
        yield return SceneManager.LoadSceneAsync(key.String);
        CutsceneManager.Instance.CurrentSetup.SetUp();
        CutsceneManager.Instance._currentCinemachine = GameObject.FindFirstObjectByType<CinemachineCamera>();
        CutsceneManager.Instance._currentCinemachine.Follow = CutsceneManager.Instance.CurrentSetup.CameraFocusPoint;
        SceneSetupInProgress = false;
    }
    public static void CleanScene()
    {
        if (CutsceneManager.Instance.CurrentSetup == null) return;
        CutsceneManager.Instance.CurrentSetup.UndoOmit();
        MonoBehaviour.Destroy(CutsceneManager.Instance.CurrentSetup.CutsceneObjects);
        CutsceneManager.Instance.CurrentSetup = null;
        CutsceneManager.Instance._currentCinemachine.GetComponent<CinemachineConfiner2D>().enabled = true;
    }

    public static void SceneSetup(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("SetupKey") == null)
            return;
        SceneSetupInProgress = true;
        CutsceneManager.Instance.FunctionYieldCheck = delegate
        {
            return SceneSetupInProgress;
        };
        GlobalCanvasManager.Instance.StartCoroutine(SetupCoroutine(cutscene,dataList));
    }
    public static void Dialogue(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("DialogueSequence") == null)
            return;
        DialogueSequence sequence = dataList.GetData("DialogueSequence").Obj as DialogueSequence;
        GlobalCanvasManager.Instance.DialogueHandler.PromptSequence(sequence);
        if (Yield)
        {
            CutsceneManager.Instance.FunctionYieldCheck = delegate
            {
                return GlobalCanvasManager.Instance.DialogueHandler.IsInProgress();
            };
        }
    }

    public static void FaceActor(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("Actor") == null || dataList.GetData("Direction") == null)
            return;
        var actor = CutsceneManager.Instance.GetActor(dataList.GetData("Actor").String);
        if (actor == null) return;
        actor.FaceActor(dataList.GetData("Direction").Int);
    }
    public static void MoveActor(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("Actor") == null || dataList.GetData("Point") == null)
            return;
        var actor = CutsceneManager.Instance.GetActor(dataList.GetData("Actor").String);
        var point = CutsceneManager.Instance.GetPoint(dataList.GetData("Point").String);
        if (actor == null || point == null) return;
        if (Yield)
        {
            CutsceneManager.Instance.FunctionYieldCheck = actor.IsInProgress;
        }

        float speed = 1;
        if (dataList.GetData("MoveSpeed") != null)
        {
            speed = dataList.GetData("MoveSpeed").Float;
        }
        actor.MoveActor(point, speed);
    }
    public static void AnimateActor(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("Actor") == null || dataList.GetData("Anim") == null)
            return;
        var actor = CutsceneManager.Instance.GetActor(dataList.GetData("Actor").String);
        if (actor == null) return;
        if (Yield)
        {
            CutsceneManager.Instance.FunctionYieldCheck = actor.IsInProgress;
        }
        actor.PlayAnimation(dataList.GetData("Anim").String);
    }
    public static void SetControllerPosition(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("Point") == null || CutsceneManager.Instance.CurrentSetup.Controller == null)
            return;
        var point = CutsceneManager.Instance.GetPoint(dataList.GetData("Point").String);
        if (point == null) return;
        CutsceneManager.Instance.CurrentSetup.Controller.position = point.position;
    }

    public static void SetCameraFocus(Cutscene cutscene, KeyDataList dataList)
    {
        var confiner = dataList.GetData("Confiner");
        if (confiner != null)
        {
            bool confine = confiner.Int >= 0;
            CutsceneManager.Instance._currentCinemachine.GetComponent<CinemachineConfiner2D>().enabled = confine;
        }
        var point = dataList.GetData("Point");
        if (point != null)
        {
            var foundPoint = CutsceneManager.Instance.GetPoint(point.String);
            if (foundPoint != null)
            {
                CutsceneManager.Instance._currentCinemachine.Follow = CutsceneManager.Instance.CurrentSetup.CameraFocusPoint;
                CutsceneManager.Instance.CurrentSetup.CameraFocusPoint.position = foundPoint.position;
                return;
            }
        }
        var actor = dataList.GetData("Actor");
        if (actor != null)
        {
            var foundActor = CutsceneManager.Instance.GetActor(actor.String);
            if (foundActor != null)
            {
                CutsceneManager.Instance._currentCinemachine.Follow = foundActor.transform;
                return;
            }
        }
    }

    public static void DestroyProp(Cutscene cutscene, KeyDataList dataList)
    {
        var prop = dataList.GetData("Prop");
        if (prop != null)
        {
            var found = CutsceneManager.Instance.GetProp(prop.String);
            MonoBehaviour.Destroy(found);
            return;
        }
        var actor = dataList.GetData("Actor");
        if (actor != null)
        {
            var found = CutsceneManager.Instance.GetActor(actor.String);
            MonoBehaviour.Destroy(found.gameObject);
            return;
        }
    }
}