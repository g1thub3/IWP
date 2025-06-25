using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DungeonTransitionHandler : MonoBehaviour
{
    [Header("Assets")]
    public CanvasGroup grp;
    public CanvasGroup floorDispGrp;
    [SerializeField] private TMP_Text _dungeonText;
    [SerializeField] private TMP_Text _floorText;

    [SerializeField] private GameObject _questCompFrame;
    [SerializeField] private Transform _compItemContainer;
    [SerializeField] private GameObject _compItem;
    [SerializeField] private GameObject _listItem;

    private DGGameManager _gameManager;

    private void Start()
    {
        grp = GetComponent<CanvasGroup>();
        _gameManager = FindAnyObjectByType<DGGameManager>();
    }

    public IEnumerator FadeTransition(bool fadeIn, float duration, float delay, CanvasGroup group, Action OnComplete)
    {
        Transition trans = new Transition();
        trans.t = 0;
        trans.max = duration;

        yield return new WaitForSeconds(delay);

        while (trans.Progression < 1)
        {
            trans.Progress();
            if (fadeIn)
            {
                group.alpha = trans.Progression;
            } else
            {
                group.alpha = 1 - trans.Progression;
            }
            yield return new WaitForEndOfFrame();
        }
        OnComplete.Invoke();
    }
    public void ToggleQuestComp(bool isActive)
    {
        _questCompFrame.SetActive(isActive);
    }
    
    public void SetDungeonText(string text)
    {
        _dungeonText.text = text;
    }
    public void SetFloorText(string text) { 
        _floorText.text = text;
    }

    public void LoadCompetition(Dictionary<Quest, List<QuestCompetitor>> questCompetition)
    {
        for (int i = _compItemContainer.childCount - 1; i >= 0; i--) { 
            Destroy(_compItemContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < _gameManager.ActiveQuests.Count; i++)
        {
            var q = _gameManager.ActiveQuests[i];
            if (questCompetition.ContainsKey(q))
            {
                var item = Instantiate(_compItem, _compItemContainer);
                TMP_Text titleObj = item.transform.Find("QuestTitle").GetComponent<TMP_Text>();

                string title = string.Empty;
                if (GlobalGameManager.Instance.selectedDungeon.isAscending)
                {
                    title = "(" + q.quest.floor + "F) ";
                } else
                {
                    title = "(B" + q.quest.floor + "F) ";
                }
                title += q.QuestObjectiveText;
                if (title.Length > 25)
                {
                    title = title.Substring(0, 25) + "...";
                }
                titleObj.text = title;

                if (q.quest.questCompleted)
                {
                    titleObj.color = Color.green;
                } else if (!q.quest.questPossible)
                {
                    titleObj.color = Color.red;
                } else
                {
                    if (_gameManager.CurrentFloor == q.quest.floor)
                        titleObj.color = Color.yellow;
                    var container = item.transform.Find("CompetitorsContainer");
                    var compList = questCompetition[q];
                    foreach (var comp in compList)
                    {
                        var listItem = Instantiate(_listItem, container);
                        TMP_Text txt = listItem.GetComponent<TMP_Text>();
                        if (GlobalGameManager.Instance.selectedDungeon.isAscending)
                        {
                            txt.text = "(" + comp.currentFloor + "F) ";
                        }
                        else
                        {
                            txt.text = "(B" + comp.currentFloor + "F) ";
                        }
                        txt.text += comp.competitorName;
                        if (comp.currentFloor == q.quest.floor)
                        {
                            txt.color = Color.red;
                        }
                        else if (comp.currentFloor == _gameManager.CurrentFloor)
                        {
                            txt.color = Color.yellow;
                        }
                    }
                }
            }
        }
    }
}
