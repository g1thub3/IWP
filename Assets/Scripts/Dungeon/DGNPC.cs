using UnityEngine;

public class DGNPC : MonoBehaviour
{
    private CharacterBehaviour _characterBehaviour;
    private DGEntity _entity;
    public DGAIModule main;
    public KeyDataList aiDataList;
    private DGGameManager _dgGameManager;

    public QuestCompetitor associatedCompetitor = null;

    private void Start()
    {
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _entity = GetComponent<DGEntity>();
        _characterBehaviour = GetComponent<CharacterBehaviour>();
    }
    private void Update()
    {
        if (_entity.IsPerformingAction || _dgGameManager.CurrentEntityTurn() != _entity || GlobalCanvasManager.Instance.IsInteractionActive) return;
        if (main != null)
        {
            main.Run(this, aiDataList);
        }
    }
}
