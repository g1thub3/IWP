using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QuestBoardHandler : MonoBehaviour
{
    [SerializeField] CanvasGroup _questBoardGrp;
    [SerializeField] TMP_Text _questBoardTitle, _questBoardPage;
    [SerializeField] Transform _questBoardContent;
    [SerializeField] GameObject _questBoardEntry;

    private PlayerInput _inputManager;
    private bool _isCompetitive;
    private int _currentPage;
    private RectTransform _selector;
    private int _currSelected;
    private bool _openToggle;

    private void Start()
    {
        _isCompetitive = false;
        _inputManager = FindAnyObjectByType<PlayerInput>();
        _currentPage = 0;
        _currSelected = 0;
        _openToggle = false;
    }

    public bool IsOpen
    {
        get { return _questBoardGrp.alpha > 0; }
    }

    private void Highlight()
    {
        if (_selector == null) return;
        _selector.GetComponent<Image>().enabled = !_selector.GetComponent<Image>().enabled;
    }
    private void Select(int inc, RectTransform newSelector = null)
    {
        Highlight();
        inc = Mathf.Clamp(inc, -1, 1);
        _currSelected += inc;
        if (_currSelected < 0)
            _currSelected = 1;
        else if (_currSelected > 1)
            _currSelected = 0;
        if (newSelector == null)
            _selector = _questBoardContent.GetChild(_currSelected).Find("Selector").GetComponent<RectTransform>();
        else
            _selector = newSelector;
        Highlight();
    }

    private void IncPage(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        _currentPage += inc;
        if (_currentPage < 0)
            _currentPage = 3;
        if (_currentPage > 3)
            _currentPage = 0;
        var newSelector = LoadEntriesOfPage();
        Select(0, newSelector);
    }
    private void ClearList()
    {
        for (int i = _questBoardContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_questBoardContent.GetChild(i).gameObject);
        }
    }
    private RectTransform LoadEntriesOfPage()
    {
        _selector = null;
        RectTransform newSelector = null;
        ClearList();
        _questBoardPage.text = (_currentPage + 1) + "/" + 4;
        for (int i = _currentPage * 2; i < _currentPage * 2 + 2; i++)
        {
            if (_isCompetitive)
            {

            } else
            {
                var questData = GlobalGameManager.Instance.availableQuests[i];
                var newEntry = Instantiate(_questBoardEntry, _questBoardContent);
                newEntry.transform.Find("QuestTitle").GetComponent<TMP_Text>().text = questData.QuestTitleText;
                newEntry.transform.Find("QuestInfo").GetComponent<TMP_Text>().text = 
                    questData.QuestClientText + "\n" + questData.QuestPlaceText + "\n" + questData.QuestObjectiveText + "\n" + questData.QuestDifficultyText + "\n" + questData.QuestRewardText;
                if (GlobalGameManager.Instance.ownedQuests.Contains(questData))
                {
                    newEntry.transform.Find("IsTaken").GetComponent<TMP_Text>().text = "Taken";
                    newEntry.transform.Find("IsTaken").GetComponent<TMP_Text>().color = Color.red;
                }
                if (i % 2 == _currSelected)
                    newSelector = newEntry.transform.Find("Selector").GetComponent<RectTransform>();
            }
        }
        return newSelector;
    }

    private void AcceptQuest()
    {
        var quest = GlobalGameManager.Instance.availableQuests[_currentPage * 2 + _currSelected];
        if (!GlobalGameManager.Instance.ownedQuests.Contains(quest))
        {
            if (GlobalGameManager.Instance.ownedQuests.Count < GlobalGameManager.maxQuestCount)
            {
                GlobalGameManager.Instance.ownedQuests.Add(quest);
                LoadEntriesOfPage();
                Select(0);
            }
        }
    }

    private void Update()
    {
        if (!IsOpen)
        {
            if (_openToggle)
            {
                _openToggle = false;
                Open();
            }

            return;
        }
        if (_inputManager.actions["Up"].WasPressedThisFrame())
        {
            Select(-1);
        }
        if (_inputManager.actions["Down"].WasPressedThisFrame())
        {
            Select(1);
        }
        if (_inputManager.actions["Left"].WasPressedThisFrame())
        {
            IncPage(-1);
        }
        if (_inputManager.actions["Right"].WasPressedThisFrame())
        {
            IncPage(1);
        }
        if (_inputManager.actions["Decline"].WasPressedThisFrame())
        {
            _questBoardGrp.alpha = 0;
            GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = true;
        }
        if (_inputManager.actions["Accept"].WasPressedThisFrame())
        {
            AcceptQuest();
        }
    }

    public void OpenToggle(bool isCompetitive)
    {
        _isCompetitive = isCompetitive;
        _openToggle = true;
    }

    public void Open()
    {
        GlobalCanvasManager.Instance.FreeRoamMenuHandler.enabled = false;
        if (_isCompetitive)
        {
            _questBoardTitle.text = "Competitive Quests";
        }
        else
        {
            _questBoardTitle.text = "Normal Quests";
        }
        var newSelector = LoadEntriesOfPage();
        Select(0, newSelector);
        _questBoardGrp.alpha = 1;
    }
}
