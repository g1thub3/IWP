using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelUpHandler : MonoBehaviour
{
    [Header("Assets")]
    [SerializeField] CanvasGroup _lvlupGrp;
    [SerializeField] CanvasGroup _continueMsg;
    [SerializeField] Transform _lvlupContainer;
    [SerializeField] GameObject _levelupMember; // note: move it right 25
    [SerializeField] CanvasGroup _advGrp;
    [SerializeField] TMP_Text _rankText;
    [SerializeField] TMP_Text _expText;
    [SerializeField] RectTransform _barAmt;

    [Header("Properties")]
    [SerializeField] private float _fadeInBG = 0.5f;
    [SerializeField] private float _fadeInStat = 0.35f;

    private bool _isPressingInit, _canSkip;
    private string[] strings = { "HP", "HG", "EN", "MN", "PA", "PD", "MA", "MD" };
    private bool _isInProgress;
    private PlayerInput _inputManager;
    private Transition _rankTrans;
    private int _check;

    public bool SequenceInProgress
    {
        get {  return _isInProgress; }
    }
    
    private void Start()
    {
        _isInProgress = false;
        _inputManager = GetComponent<PlayerInput>();
        _rankTrans = new Transition();
        _rankTrans.max = 0.5f;
        _check = 0;

        _isPressingInit = false;
        _canSkip = false;
    }

    public void LevelUpSequence(List<CharacterEntry> characters, List<int> changes)
    {
        _isInProgress = true;
        _continueMsg.alpha = 0.0f;
        _lvlupGrp.alpha = 0.0f;

        _isPressingInit = _inputManager.actions["Accept"].IsPressed();
        _canSkip = false;

        for (int i = _lvlupContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_lvlupContainer.GetChild(i).gameObject);
        }

        for (int i = 0; i < characters.Count; i++) { 
            var character = characters[i];
            var newEntry = Instantiate(_levelupMember, _lvlupContainer);
            Transform newTransform = newEntry.transform;
            newTransform.Find("CharacterSprite").Find("Sprite").GetComponent<Image>().sprite = character.Profile.characterSprite;
            newTransform.Find("CharacterName").GetComponent<TMP_Text>().text = character.Profile.characterName;

            newTransform.Find("Lvl").GetComponent<TMP_Text>().text = "Lv. " + (character.characterLevel - changes[i]) + " > " + character.characterLevel;
            newTransform.Find("Lvl").Find("Add").GetComponent<TMP_Text>().text = "+" + changes[i];

            CharacterStat[] stats = { character.maxHealth, character.hungerSize, character.maxEnergy, character.maxMana, character.physAtk, character.physDef, character.magicAtk, character.magicDef };
            int[] oldStats = new int[stats.Length];
            int[] newStats = new int[stats.Length];
            for (int j = 0; j < stats.Length; j++)
            {
                stats[j].CalculateAfterLevel(character.characterLevel - changes[i]);
                oldStats[j] = stats[j].CurrStat;
                stats[j].CalculateAfterLevel(character.characterLevel);
                newStats[j] = stats[j].CurrStat;

                newTransform.Find(strings[j]).GetComponent<TMP_Text>().text = strings[j] + ": " + oldStats[j] + " > " + newStats[j];
                newTransform.Find(strings[j]).Find("Add").GetComponent<TMP_Text>().text = "+" + (newStats[j] - oldStats[j]);
            }

            character.Recalculate();
        }
        StartCoroutine(LevelUpCoroutine());
    }

    private IEnumerator LevelUpCoroutine()
    {
        Transition grpTrans = new Transition();
        grpTrans.max = _fadeInBG;
        while (grpTrans.Progression < 1)
        {
            grpTrans.Progress();
            _lvlupGrp.alpha = grpTrans.Progression;
            if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
            {
                _isPressingInit = true;
                _canSkip = true;
            } else
            {
                _isPressingInit = false;
            }
            if (!_canSkip)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        _canSkip = false;
        _isPressingInit = _inputManager.actions["Accept"].IsPressed();

        Transition itemTrans = new Transition();
        itemTrans.max = _fadeInStat;
        for (int i = 0; i < _lvlupContainer.childCount; i++)
        {
            var item = _lvlupContainer.GetChild(i);
            var grp = item.GetComponent<CanvasGroup>();
            while (itemTrans.Progression < 1)
            {
                itemTrans.Progress();
                grp.alpha = itemTrans.Progression;
                if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
                {
                    _isPressingInit = true;
                    _canSkip = true;
                }
                else
                {
                    _isPressingInit = false;
                }
                if (!_canSkip)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            itemTrans.t = 0;

            var lvlGrp = item.Find("Lvl").GetComponent<CanvasGroup>();
            while (itemTrans.Progression < 1)
            {
                itemTrans.Progress();
                lvlGrp.alpha = itemTrans.Progression;
                if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
                {
                    _isPressingInit = true;
                    _canSkip = true;
                }
                else
                {
                    _isPressingInit = false;
                }
                if (!_canSkip)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            itemTrans.t = 0;

            var lvlGrpAdd = lvlGrp.transform.Find("Add");
            var addGrp = lvlGrpAdd.GetComponent<CanvasGroup>();
            var addRT = addGrp.GetComponent<RectTransform>();
            Vector2 ogPos = addRT.position;
            while (itemTrans.Progression < 1)
            {
                itemTrans.Progress();
                addGrp.alpha = itemTrans.Progression;
                addRT.position = new Vector2(ogPos.x + (25.0f * itemTrans.Progression), ogPos.y);
                if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
                {
                    _isPressingInit = true;
                    _canSkip = true;
                }
                else
                {
                    _isPressingInit = false;
                }
                if (!_canSkip)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            itemTrans.t = 0;


            foreach (var str in strings)
            {
                var statGrp = item.Find(str).GetComponent<CanvasGroup>();
                while (itemTrans.Progression < 1)
                {
                    itemTrans.Progress();
                    statGrp.alpha = itemTrans.Progression;
                    if (_inputManager.actions["Accept"].IsPressed() && !_isPressingInit)
                    {
                        _isPressingInit = true;
                        _canSkip = true;
                    }
                    else
                    {
                        _isPressingInit = false;
                    }
                    if (!_canSkip)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                itemTrans.t = 0;

                var statGrpAdd = statGrp.transform.Find("Add");
                addGrp = statGrpAdd.GetComponent<CanvasGroup>();
                addRT = addGrp.GetComponent<RectTransform>();
                ogPos = addRT.position;
                while (itemTrans.Progression < 1)
                {
                    itemTrans.Progress();
                    addGrp.alpha = itemTrans.Progression;
                    addRT.position = new Vector2(ogPos.x + (25.0f * itemTrans.Progression), ogPos.y);
                    if (!_canSkip)
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }
                itemTrans.t = 0;
            }
        }

        grpTrans.t = 0;
        while (grpTrans.Progression < 1)
        {
            grpTrans.Progress();
            _continueMsg.alpha = grpTrans.Progression;
            if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
            {
                _isPressingInit = true;
                _canSkip = true;
            }
            else
            {
                _isPressingInit = false;
            }
            if (!_canSkip)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        while (!_inputManager.actions["Accept"].IsPressed() || _isPressingInit)
        {
            if (!_inputManager.actions["Accept"].IsPressed())
                _isPressingInit = false;
            yield return new WaitForEndOfFrame();
        }

        _continueMsg.alpha = 0;
        _lvlupGrp.alpha = 0;
        for (int i = _lvlupContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_lvlupContainer.GetChild(i).gameObject);
        }
        _isInProgress = false;
    }
    public void RankUpSequence(Dictionary<string, int> data)
    {
        _isInProgress = true;
        _continueMsg.alpha = 0.0f;
        _lvlupGrp.alpha = 0.0f;
        _advGrp.alpha = 0.0f;

        _isPressingInit = _inputManager.actions["Accept"].IsPressed();
        _canSkip = false;

        _rankText.text = data["OldRank"].ToString();
        _expText.text = data["OldEXP"] + " / " + GlobalGameManager.Instance.GetExpToNextRank(data["OldRank"]);
        float perc = (float)data["OldEXP"] / GlobalGameManager.Instance.GetExpToNextRank(data["OldRank"]);
        _barAmt.sizeDelta = new Vector2(_barAmt.sizeDelta.x, 1500.0f * perc);

        StartCoroutine(RankUpCoroutine(data));
    }

    // Rank font: 125 > 175, bar size: 1500
    private IEnumerator RankUpCoroutine(Dictionary<string, int> data)
    {
        Transition grpTrans = new Transition();
        grpTrans.max = _fadeInBG;
        while (grpTrans.Progression < 1)
        {
            grpTrans.Progress();
            _lvlupGrp.alpha = grpTrans.Progression;
            if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
            {
                _isPressingInit = true;
                _canSkip = true;
            }
            else
            {
                _isPressingInit = false;
            }
            if (!_canSkip)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        grpTrans.t = 0;
        while (grpTrans.Progression < 1)
        {
            grpTrans.Progress();
            _advGrp.alpha = grpTrans.Progression;
            if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
            {
                _isPressingInit = true;
                _canSkip = true;
            }
            else
            {
                _isPressingInit = false;
            }
            if (!_canSkip)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        if (!_canSkip)
        {
            yield return new WaitForSeconds(0.5f);
        }

        _isPressingInit = _inputManager.actions["Accept"].IsPressed();
        _canSkip = false;

        Transition itemTrans = new Transition();
        itemTrans.max = 0.5f;

        int currRank = data["OldRank"];
        int req = GlobalGameManager.Instance.GetExpToNextRank(currRank);
        int oldExp = data["OldEXP"];
        int currExp = oldExp;
        int addedExp = data["AddedEXP"];

        while (itemTrans.Progression < 1)
        {
            if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
            {
                _isPressingInit = true;
                _canSkip = true;
            }
            else
            {
                _isPressingInit = false;
            }
            itemTrans.Progress();
            currExp = (int)Mathf.Floor(oldExp + (addedExp * itemTrans.Progression));

            if (currExp >= req)
            {
                currRank++;
                _rankText.text = currRank.ToString();
                StartCoroutine(RankTextCoroutine());
                req = GlobalGameManager.Instance.GetExpToNextRank(currRank);
                int change = currExp - oldExp;
                oldExp = 0;
                currExp = 0;
                addedExp -= change;
                itemTrans.t = 0;
            }

            _expText.text = currExp + " / " + req;

            float perc = (float)currExp / req;
            _barAmt.sizeDelta = new Vector2(_barAmt.sizeDelta.x, 1500.0f * perc);

            if (!_canSkip)
            {
                yield return new WaitForEndOfFrame();
            }
        }


        grpTrans.t = 0;
        while (grpTrans.Progression < 1)
        {
            if (_inputManager.actions["Accept"].WasPressedThisFrame() && !_isPressingInit)
            {
                _isPressingInit = true;
                _canSkip = true;
            }
            else
            {
                _isPressingInit = false;
            }
            grpTrans.Progress();
            _continueMsg.alpha = grpTrans.Progression;
            if (!_canSkip)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        _isPressingInit = _inputManager.actions["Accept"].IsPressed();
        while (!_inputManager.actions["Accept"].IsPressed() || _isPressingInit)
        {
            if (!_inputManager.actions["Accept"].WasPressedThisFrame())
                _isPressingInit = false;
            yield return new WaitForEndOfFrame();
        }

        _continueMsg.alpha = 0;
        _lvlupGrp.alpha = 0;
        _advGrp.alpha = 0.0f;
        _isInProgress = false;
    }

    private IEnumerator RankTextCoroutine()
    {
        _rankTrans.t = 0;
        _check++;
        int check = _check;
        while (check == _check && _rankTrans.Progression < 1)
        {
            _rankTrans.Progress();
            _rankText.fontSize = 175 - (50 * _rankTrans.Progression);
            yield return new WaitForEndOfFrame();
        }
    }
}
