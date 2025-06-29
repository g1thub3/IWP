using TMPro;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    [SerializeField] CanvasGroup _grp;
    public TMP_Text text;
    [SerializeField] private float _uptime = 0.5f;
    [SerializeField] private float _tweenTime = 0.25f;
    [SerializeField] private float _inc = 0.2f;
    private Transition _trans1 , _trans2;
    private void Start()
    {
        _trans1 = new Transition();
        _trans1.max = _uptime;
        _trans2 = new Transition();
        _trans2.max = _tweenTime;
        Destroy(transform.gameObject, _uptime + _tweenTime + 0.25f);
    }
    private void Update()
    {
        transform.position += new Vector3(0, _inc) * Time.deltaTime;
        if (_trans1.Progression < 1)
        {
            _trans1.Progress();
        } else
        {
            _trans2.Progress();
            _grp.alpha = 1 - _trans2.Progression;
        }
    }
}
