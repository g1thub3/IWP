using System.Collections.Generic;
using UnityEngine;

public class DGNPC : MonoBehaviour
{
    private DGEntity _entity;
    public DGAIModule main;
    public KeyDataList aiDataList;
    private DGGameManager _dgGameManager;
    public QuestCompetitor associatedCompetitor = null;
    public bool isQuestTarget = false;
    private void Start()
    {
        _dgGameManager = FindAnyObjectByType<DGGameManager>();
        _entity = GetComponent<DGEntity>();
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
