using TMPro;
using UnityEngine;

public class DungeonIntro : MonoBehaviour
{
    [SerializeField] GameObject[] _hidden;
    [SerializeField] TMP_Text _dungeonText;
    [SerializeField] Animator[] _animators;
    [SerializeField] SpriteRenderer _bg1, _bg2, _bg3, _bg4, _bg5;
    [SerializeField] float _spd1, _spd2, _spd3, _spd4, _spd5;

    private void Start()
    {
        _dungeonText.text = "Travelling to " + GlobalGameManager.Instance.selectedDungeon.dungeonName + "...";
        for (int i = 0; i < _animators.Length; i++)
        {
            if (i >= GlobalGameManager.Instance.party.Count)
            {
                _animators[i].gameObject.SetActive(false);
                continue;
            } else
            {
                _animators[i].runtimeAnimatorController = GlobalGameManager.Instance.party[i].Profile.animatorController;
                _animators[i].Play("walk_east");
                _animators[i].gameObject.SetActive(true);
            }
        }
        foreach (var obj in _hidden)
        {
            obj.SetActive(false);
        }
    }

    public void IntroComplete()
    {
        foreach (var obj in _hidden)
        {
            obj.SetActive(true);
        }
        Destroy(gameObject);
    }

    void Update()
    {
        _bg1.size += new Vector2(_spd1 * Time.deltaTime,0);
        _bg2.size += new Vector2(_spd2 * Time.deltaTime, 0);
        _bg3.size += new Vector2(_spd3 * Time.deltaTime, 0);
        _bg4.size += new Vector2(_spd4 * Time.deltaTime, 0);
        _bg5.size += new Vector2(_spd5 * Time.deltaTime, 0);
    }
}
