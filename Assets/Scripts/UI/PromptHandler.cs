using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PromptInfo
{
    public string message;
    public string[] options;
}
public class OptionAsset
{
    public GameObject bobber;
    public TMP_Text optiontext;
    public float basePos;
}

public class PromptHandler : MonoBehaviour, IDebuggable
{
    [Header("Assets")]
    [SerializeField] CanvasGroup _promptMsgGrp;
    [SerializeField] TMP_Text _promptMessage;
    [SerializeField] CanvasGroup _optionListGrp;
    [SerializeField] GameObject _optionPrefab;
    [SerializeField] Color _selectionColour;

    [Header("Properties")]
    [SerializeField] private float _bobInPrSpeed = 5;
    [SerializeField] private float _bobInPrAmount = 3;
    [SerializeField] private float _transitionTime = 0.5f;
    [SerializeField] private float _textWait = 0.3f;

    private OptionAsset _currentlySelectedAsset;
    Transition _msgTr, _optTr;
    private bool _prompting;
    private int _returnedAnswer;
    private PlayerInput _inputManager;

    public int TakeAnswer()
    {
        int takenAnswer = _returnedAnswer;
        _returnedAnswer = -1;
        return takenAnswer;
    }
    public bool IsPromptInProgress
    {
        get { return _prompting; }
    }

    private void Refresh()
    {
        _promptMessage.text = "";
        for (int i = _optionListGrp.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_optionListGrp.transform.GetChild(i).gameObject);
        }
    }

    private OptionAsset[] FillOptions(string[] options)
    {
        OptionAsset[] assets = new OptionAsset[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            var newAsset = new OptionAsset();
            var obj = Instantiate(_optionPrefab, _optionListGrp.transform);
            newAsset.optiontext = obj.GetComponentInChildren<TMP_Text>();
            newAsset.optiontext.text = options[i];
            newAsset.bobber = obj.transform.Find("Selector").gameObject;
            newAsset.bobber.SetActive(false);
            newAsset.basePos = newAsset.bobber.GetComponent<RectTransform>().position.x + 500;
            assets[i] = newAsset;
        }
        return assets;
    }

    public void Prompt(PromptInfo prompt, CanvasGroup[] hidden = null)
    {
        if (_prompting) return;
        _prompting = true;
        _returnedAnswer = -1;
        //Hide UIs
        if (hidden != null)
        {
            foreach (var grp in hidden)
            {
                grp.alpha = 0;
            }
        }
        //Handle prompt

        StartCoroutine(HandlePrompt(prompt, hidden));
    }

    private IEnumerator HandlePrompt(PromptInfo prompt, CanvasGroup[] hidden)
    {
        Refresh();
        _msgTr.t = 0;
        while (_msgTr.Progression < 1)
        {
            _msgTr.Progress();
            _promptMsgGrp.alpha = _msgTr.Progression;
            yield return new WaitForEndOfFrame();
        }

        _optTr.t = 0;
        OptionAsset[] assets = FillOptions(prompt.options);

        bool skip = false;
        int txtCount = 0;
        while (txtCount < prompt.message.Length)
        {
            _promptMessage.text += prompt.message[txtCount];
            txtCount++;
            if (!skip)
            {
                AudioManager.Instance.PlaySFXInScreen("TextSFX");
                skip = _inputManager.actions["Accept"].IsPressed();
                yield return new WaitForSeconds(_textWait);
            }
        }

        yield return new WaitForSeconds(0.5f);

        while (_optTr.Progression < 1)
        {
            _optTr.Progress();
            _optionListGrp.alpha = _optTr.Progression;
            yield return new WaitForEndOfFrame();
        }

        int currentSelection = -2;
        bool firstInput = true;
        while (_returnedAnswer < 0)
        {
            bool inputFound = false;
            if (_inputManager.actions["Up"].IsPressed())
            {
                currentSelection++;
                inputFound = true;
            }
            if (_inputManager.actions["Down"].IsPressed())
            {
                inputFound = true;
                currentSelection--;
            }
            if (inputFound)
            {
                if (currentSelection < 0)
                    currentSelection = prompt.options.Length - 1;
                else if (currentSelection >= prompt.options.Length)
                    currentSelection = 0;
                if (_currentlySelectedAsset != null)
                {
                    _currentlySelectedAsset.optiontext.color = Color.white;
                    _currentlySelectedAsset.bobber.SetActive(false);
                }
                _currentlySelectedAsset = assets[currentSelection];
                _currentlySelectedAsset.optiontext.color = _selectionColour;
                _currentlySelectedAsset.bobber.SetActive(true);
                AudioManager.Instance.PlaySFXInScreen("Select");
                yield return new WaitForSeconds(0.2f);
            }

            if (_inputManager.actions["Accept"].IsPressed())
            {
                if (currentSelection < 0 || currentSelection >= prompt.options.Length)
                {
                    if (!firstInput)
                    {
                        AudioManager.Instance.PlaySFXInScreen("Error");
                        firstInput = true;
                        // PLAY ERROR SOUND
                    }
                } else
                {
                    _returnedAnswer = currentSelection;
                    AudioManager.Instance.PlaySFXInScreen("Confirm");
                }
                yield return new WaitForSeconds(0.1f);
            } else
            {
                firstInput = false;
            }
            yield return new WaitForEndOfFrame();
        }

        _promptMsgGrp.alpha = 0;
        _optionListGrp.alpha = 0;
        _currentlySelectedAsset = null;

        //Unhide UIs
        _prompting = false;
        if (hidden != null)
        {
            foreach (var grp in hidden)
            {
                grp.alpha = 1;
            }
        }
    }

    private void Start()
    {
        _msgTr = new Transition();
        _msgTr.max = _transitionTime;
        _optTr = new Transition();
        _optTr.max = _transitionTime;
        _inputManager = GetComponent<PlayerInput>();
    }


    private void Bobber()
    {
        if (_currentlySelectedAsset == null) return;
        _currentlySelectedAsset.bobber.GetComponent<RectTransform>().position = 
            new Vector2(_currentlySelectedAsset.basePos + (_bobInPrAmount * 0.5f - (_bobInPrAmount * Mathf.Sin(_bobInPrSpeed * Time.time))),
            _currentlySelectedAsset.bobber.GetComponent<RectTransform>().position.y);
    }

    private void Update()
    {
        DebugControls();
        Bobber();
    }

    public void DebugControls()
    {
        if (!DebugTools.Instance.PromptDebugOn) return;
        if (Input.GetKeyDown(KeyCode.M))
        {
            PromptInfo newPr = new PromptInfo();
            newPr.message = "Do you wanna prompt?";
            newPr.options = new string[2];
            newPr.options[0] = "Yuuuup";
            newPr.options[1] = "Naaaah man";
            Prompt(newPr);
            StartCoroutine(test());
        }
    }

    private IEnumerator test()
    {
        while (IsPromptInProgress)
        {
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("ANSWER: " + TakeAnswer());
    }
}
