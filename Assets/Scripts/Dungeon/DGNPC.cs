using UnityEngine;

public class DGNPC : MonoBehaviour
{
    private CharacterBehaviour _characterBehaviour;
    private DGEntity _entity;
    public DGAIModule main;
    public KeyDataList aiDataList;
    private Animator _animator;
    private DGGameManager _dgGameManager;

    private void Start()
    {
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _entity = GetComponent<DGEntity>();
        _characterBehaviour = GetComponent<CharacterBehaviour>();
        _animator = GetComponent<Animator>();
        if (_characterBehaviour.character.associatedCharacter != CHARACTER_ENUM.NUM_CHARACTERS)
        {
            main = CharacterProfiles.Instance.characterProfiles[(int)_characterBehaviour.character.associatedCharacter].aiModule;
        }
    }
    private void Update()
    {
        if (_entity.IsPerformingAction || _dgGameManager.CurrentEntityTurn() != _entity) return;
        if (main != null)
        {
            main.Run(this, aiDataList);
        }
    }
}
