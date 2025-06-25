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

    [Header("Properties")]
    [SerializeField] private float _fadeInBG = 0.5f;
    [SerializeField] private float _fadeInStat = 0.35f;

    private string[] strings = { "HP", "HG", "EN", "MN", "PA", "PD", "MA", "MD" };
    private bool _isInProgress;
    private PlayerInput _inputManager;

    public bool SequenceInProgress
    {
        get {  return _isInProgress; }
    }
    private void Start()
    {
        _isInProgress = false;
        _inputManager = GetComponent<PlayerInput>();
    }

    public void LevelUpSequence(List<CharacterEntry> characters, List<int> changes)
    {
        _isInProgress = true;
        _continueMsg.alpha = 0.0f;
        _lvlupGrp.alpha = 0.0f;

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
            yield return new WaitForEndOfFrame();
        }

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
                yield return new WaitForEndOfFrame();
            }
            itemTrans.t = 0;

            var lvlGrp = item.Find("Lvl").GetComponent<CanvasGroup>();
            while (itemTrans.Progression < 1)
            {
                itemTrans.Progress();
                lvlGrp.alpha = itemTrans.Progression;
                yield return new WaitForEndOfFrame();
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
                yield return new WaitForEndOfFrame();
            }
            itemTrans.t = 0;


            foreach (var str in strings)
            {
                var statGrp = item.Find(str).GetComponent<CanvasGroup>();
                while (itemTrans.Progression < 1)
                {
                    itemTrans.Progress();
                    statGrp.alpha = itemTrans.Progression;
                    yield return new WaitForEndOfFrame();
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
                    yield return new WaitForEndOfFrame();
                }
                itemTrans.t = 0;
            }
        }

        grpTrans.t = 0;
        while (grpTrans.Progression < 1)
        {
            grpTrans.Progress();
            _continueMsg.alpha = grpTrans.Progression;
            yield return new WaitForEndOfFrame();
        }

        while (!_inputManager.actions["Accept"].IsPressed())
        {
            yield return new WaitForEndOfFrame();
        }

        _continueMsg.alpha = 0;
        _lvlupGrp.alpha = 0;
        _isInProgress = false;
    }
}
