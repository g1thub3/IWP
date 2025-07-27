using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseHandler : MonoBehaviour
{
    [SerializeField] GameObject _frame;
    [SerializeField] GameObject _instructions, _confirmed;
    [SerializeField] RectTransform _progression, _amount;
    [SerializeField] float _holdTime = 2.0f;
    [SerializeField] float _full = 1600;
    [SerializeField] float _quitTime = 5.0f;

    private float _holding;
    private bool _isClose;
    private bool _isQuit;
    private bool _isQuitEnter;
    private Transition _quitWait;

    public bool IsOpen
    {
        get { return _frame.activeSelf; }
    }

    private void SetUp()
    {
        _isClose = false;
        _isQuit = false;
        _quitWait = new Transition();
        _quitWait.max = _quitTime;
        SetBar(0.0f);
    }
    public void Open()
    {
        _frame.SetActive(true);
        _instructions.SetActive(true);
        _confirmed.SetActive(false);
        _isQuitEnter = Input.GetKey(KeyCode.Escape);
        SetUp();
    }

    private void SetBar(float amt)
    {
        _holding = amt;
        if (_holding > 0.0f)
        {
            _progression.gameObject.SetActive(true);
            _amount.sizeDelta = new Vector2(50 + _full * (_holding / _holdTime), _amount.sizeDelta.y);
        } else
        {
            _progression.gameObject.SetActive(false);
        }
    }

    private void Quit()
    {
        _isQuit = true;
        _instructions.SetActive(false);
        _confirmed.SetActive(true);
    }

    private void Start()
    {
        SetUp();
    }

    private void Update()
    {
        if (!IsOpen)
            return;
        if (_isQuit)
        {
            _quitWait.Progress();
            if (_quitWait.Progression >= 1.0f)
            {
                Application.Quit();
            }
            return;
        }
        if (_isClose)
        {
            _frame.SetActive(false);
            return;
        }
        if (Input.GetKey(KeyCode.Escape) && !_isQuitEnter)
        {
            SetBar(_holding + Time.deltaTime);
            if (_holding >= _holdTime)
            {
                Quit();
                return;
            }
        } else
        {
            _isQuitEnter = false;
            SetBar(0.0f);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            _isClose = true;
        }
    }
}
