using System.Collections;
using UnityEngine;

public class DungeonEnter : MonoBehaviour
{
    private Collider2D _col;
    private Transform _playerCharacter;
    [SerializeField] private float _distTolerance = 3.5f;
    private void Start()
    {
        _col = GetComponent<Collider2D>();
        _playerCharacter = FindAnyObjectByType<FRController>().transform;
    }

    private void Update()
    {
        if (!_col.enabled)
            if ((transform.position - _playerCharacter.position).magnitude > _distTolerance)
                _col.enabled = true;
    }

    private IEnumerator ProcessPrompt()
    {
        while (GlobalCanvasManager.Instance.PromptHandler.IsPromptInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        int ans = GlobalCanvasManager.Instance.PromptHandler.TakeAnswer();
        if (ans < GlobalGameManager.Instance.availableDungeons.Count)
        {
            GlobalGameManager.Instance.selectedDungeon = GlobalGameManager.Instance.availableDungeons[ans];
            GameSceneManager.Instance.ToDungeon();
        }
    }

    public void DungeonPrompt()
    {
        _col.enabled = false;
        PromptInfo newInfo = new PromptInfo();
        newInfo.message = "Which dungeon do you want to explore?";
        newInfo.options = new string[GlobalGameManager.Instance.availableDungeons.Count + 1];
        for (int i = 0; i < GlobalGameManager.Instance.availableDungeons.Count; i++)
        {
            newInfo.options[i] = GlobalGameManager.Instance.availableDungeons[i].dungeonName;
        }
        newInfo.options[GlobalGameManager.Instance.availableDungeons.Count] = "Cancel";
        GlobalCanvasManager.Instance.PromptHandler.Prompt(newInfo);
        StartCoroutine(ProcessPrompt());
    }
}
