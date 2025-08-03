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

    public void DungeonPrompt()
    {
        _col.enabled = false;
        PromptInfo newPrompt = PromptInfo.New("Which dungeon do you want to explore?", 
            new string[GlobalGameManager.Instance.availableDungeons.Count + 1], 
            new PromptInfo.OptionFunction[GlobalGameManager.Instance.availableDungeons.Count + 1]);
        for (int i = 0; i < GlobalGameManager.Instance.availableDungeons.Count; i++)
        {
            newPrompt.options[i] = GlobalGameManager.Instance.availableDungeons[i].dungeonName;
            int index = i;
            newPrompt.optionFunctions[i] = delegate {
                GlobalGameManager.Instance.selectedDungeon = GlobalGameManager.Instance.availableDungeons[index];
                GameSceneManager.Instance.ToDungeon();
            };
        }
        newPrompt.options[GlobalGameManager.Instance.availableDungeons.Count] = "Cancel";
        newPrompt.optionFunctions[GlobalGameManager.Instance.availableDungeons.Count] = PromptInfo.NullFunction;
        GlobalCanvasManager.Instance.PromptHandler.Prompt(newPrompt);
    }
}
