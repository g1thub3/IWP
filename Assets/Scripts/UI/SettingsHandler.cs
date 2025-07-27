using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [SerializeField] GameObject _frame;
    [SerializeField] Slider _sfxSlider, _bgmSlider;
    [SerializeField] GameObject _sfxArrow, _bgmArrow;
    [SerializeField] float _holdTolerance = 2.0f;
    private PlayerInput _inputManager;
    private bool _isClose, _isBGMSelected;
    private float _holdTolerating;

    public bool IsOpen
    {
        get { return _frame.activeSelf; }
    }

    private void ChangeSliderValue(int inc)
    {
        float amt = Time.deltaTime * inc;
        if (_isBGMSelected)
        {
            AudioManager.Instance.BGMVolume = Mathf.Clamp(AudioManager.Instance.BGMVolume + amt, 0, 1);
            _bgmSlider.value = AudioManager.Instance.BGMVolume;
        } else
        {
            AudioManager.Instance.SFXVolume = Mathf.Clamp(AudioManager.Instance.SFXVolume + amt, 0, 1);
            _sfxSlider.value = AudioManager.Instance.SFXVolume;
        }
    }

    private void HighlightSlider()
    {
        if (_isBGMSelected)
        {
            _bgmArrow.SetActive(true);
            _sfxArrow.SetActive(false);
        }
        else
        {
            _bgmArrow.SetActive(false);
            _sfxArrow.SetActive(true);
        }
    }
    private void SwitchSlider()
    {
        AudioManager.Instance.PlaySFXInScreen("Select");
        _isBGMSelected = !_isBGMSelected;
        HighlightSlider();
    }

    public void Open()
    {
        _frame.SetActive(true);
        SetUp();
    }

    private void SetUp()
    {
        _sfxSlider.value = AudioManager.Instance.SFXVolume;
        _bgmSlider.value = AudioManager.Instance.BGMVolume;
        _isClose = false;
        _isBGMSelected = false;
        HighlightSlider();
    }

    private void Start()
    {
        _inputManager = GetComponent<PlayerInput>();
        _holdTolerating = 0.0f;
        SetUp();
    }

    private void Update()
    {
        if (!IsOpen)
            return;
        if (_isClose)
        {
            _frame.SetActive(false);
            return;
        }
        if (_inputManager.actions["Decline"].WasPressedThisFrame())
        {
            _isClose = true;
            AudioManager.Instance.PlaySFXInScreen("Close");
            return;
        }
        if (_inputManager.actions["Up"].WasPressedThisFrame() || _inputManager.actions["Down"].WasPressedThisFrame())
            SwitchSlider();
        if (_inputManager.actions["Left"].WasPressedThisFrame())
        {
            AudioManager.Instance.PlaySFXInScreen("TextSFX");
            ChangeSliderValue(-1);
        }
        if (!_inputManager.actions["Right"].IsPressed())
        {
            if (_inputManager.actions["Left"].IsPressed())
            {
                _holdTolerating += Time.deltaTime;
                if (_holdTolerating >= _holdTolerance)
                {
                    ChangeSliderValue(-1);
                }
            }
            else
            {
                if (_holdTolerating > 0.0f)
                    AudioManager.Instance.PlaySFXInScreen("TextSFX");
                _holdTolerating = 0.0f;
            }
        }
        if (_inputManager.actions["Right"].WasPressedThisFrame())
        {
            AudioManager.Instance.PlaySFXInScreen("TextSFX");
            ChangeSliderValue(1);
        }
        if (!_inputManager.actions["Left"].IsPressed())
        {
            if (_inputManager.actions["Right"].IsPressed())
            {
                _holdTolerating += Time.deltaTime;
                if (_holdTolerating >= _holdTolerance)
                {
                    ChangeSliderValue(1);
                }
            }
            else
            {
                if (_holdTolerating > 0.0f)
                    AudioManager.Instance.PlaySFXInScreen("TextSFX");
                _holdTolerating = 0.0f;
            }
        }
    }
}
