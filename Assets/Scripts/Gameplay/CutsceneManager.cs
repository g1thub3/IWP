using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;



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

    private bool _isCutsceneRunning = false;
    private CinemachineCamera _currentCinemachine;

    public void CutsceneStart()
    {
        _isCutsceneRunning = true;
    }
    public void CutsceneEnd()
    {
        _isCutsceneRunning = false;
    }
}


public enum CUTSCENE_FUNCTION
{
    SCENESETUP,
    DIALOGUE
}

public static class CutsceneFunctions
{
    private static bool Yield;
    public static void Instantiate()
    {
        Yield = false;
        functions = new Dictionary<CUTSCENE_FUNCTION, System.Action<Cutscene, KeyDataList>>();
        functions.Add(CUTSCENE_FUNCTION.SCENESETUP, SceneSwitch);
        functions.Add(CUTSCENE_FUNCTION.DIALOGUE, Dialogue);
    }
    public static Dictionary<CUTSCENE_FUNCTION, System.Action<Cutscene, KeyDataList>> functions;
    public static void Run(CutsceneInstruction instruction, Cutscene cutscene)
    {
        Yield = instruction.Yield;
        functions[instruction.function].Invoke(cutscene, instruction.Data);
    }

    public static void SceneSwitch(Cutscene cutscene, KeyDataList dataList)
    {
        CutsceneManager.Instance.FunctionYieldCheck = null;
        // Set up stuff
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
}