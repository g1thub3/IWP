using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;



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
}


public enum CUTSCENE_FUNCTION
{
    SCENESETUP,
    DIALOGUE,
    ACTOR_FACE,
    ACTOR_MOVE,
    ACTOR_ANIMATE
}

public static class CutsceneFunctions
{
    private static bool Yield;
    private static bool SceneSetupFinish;
    public static void Instantiate()
    {
        Yield = false;
        functions = new Dictionary<CUTSCENE_FUNCTION, System.Action<Cutscene, KeyDataList>>();
        functions.Add(CUTSCENE_FUNCTION.SCENESETUP, SceneSetup);
        functions.Add(CUTSCENE_FUNCTION.DIALOGUE, Dialogue);
        functions.Add(CUTSCENE_FUNCTION.ACTOR_FACE, FaceActor);
        functions.Add(CUTSCENE_FUNCTION.ACTOR_MOVE, MoveActor);
        functions.Add(CUTSCENE_FUNCTION.ACTOR_ANIMATE, AnimateActor);
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
        CutsceneManager.Instance.CurrentSetup = cutscene.FindSetup(dataList.GetData("SetupKey").String);
        GameSceneManager.Instance.previousArea = SceneManager.GetActiveScene().name;
        yield return SceneManager.LoadSceneAsync(dataList.GetData("SetupKey").String);
        CutsceneManager.Instance._currentCinemachine = GameObject.FindFirstObjectByType<CinemachineCamera>();
        CutsceneManager.Instance.CurrentSetup.SetUp();
        CutsceneManager.Instance._currentCinemachine.Follow = CutsceneManager.Instance.CurrentSetup.CameraFocusPoint;
        SceneSetupFinish = true;
    }
    public static void CleanScene()
    {
        if (CutsceneManager.Instance.CurrentSetup == null) return;
        CutsceneManager.Instance.CurrentSetup.UndoOmit();
        MonoBehaviour.Destroy(CutsceneManager.Instance.CurrentSetup.CutsceneObjects);
        CutsceneManager.Instance.CurrentSetup = null;
    }

    public static void SceneSetup(Cutscene cutscene, KeyDataList dataList)
    {
        if (dataList.GetData("SetupKey") == null)
            return;
        SceneSetupFinish = false;
        CutsceneManager.Instance.FunctionYieldCheck = delegate
        {
            return SceneSetupFinish;
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
}