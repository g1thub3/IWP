using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueHandler : MonoBehaviour, IDebuggable
{
    [Header("UI Assets")]
    [SerializeField] private CanvasGroup _dialogueFrame;
    [SerializeField] private CanvasGroup _leftMugCG;
    [SerializeField] private Image _leftSpeakerImg;
    [SerializeField] private TMP_Text _leftSpeakerName;
    [SerializeField] private CanvasGroup _rightMugCG;
    [SerializeField] private Image _rightSpeakerImg;
    [SerializeField] private TMP_Text _rightSpeakerName;
    [SerializeField] private TMP_Text _dialogueContent;
    [SerializeField] private RectTransform _dialogueContainer;
    [SerializeField] private GameObject _interactPrompt;

    [Header("Properties")]
    [SerializeField] private float _bobInPrSpeed;
    [SerializeField] private float _bobInPrAmount;
    [SerializeField] private float _transitionTime;

    
    private PlayerInput _inputManager;
    private Transition _dialogueTransition;
    private float _baseInPrPosition;

    private bool sequenceRunning = false;
    private bool dialogueRunning = false;
    private bool dialogueSkipped = false;
    private bool prevDialogueSkipped = false;

    public bool IsSequenceRunning
    {
        get { return sequenceRunning; }
    }

    public void DebugControls()
    {
        if (!DebugTools.Instance.DialogueDebugOn) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RevealSpeaker(false, false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RevealSpeaker(false, true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RevealSpeaker(true, false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            RevealSpeaker(true, true);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            RevealSpeaker(false, true, false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            RevealSpeaker(true, true, false);
        }
        if (Input.GetKeyDown(KeyCode.H) && !sequenceRunning)
        {
            PromptSequence(DebugTools.Instance.testDialogueSequence);
        }

    }

    private void RevealSpeaker(bool isRight, bool isRevealed, bool isSpriteShowing = true)
    {
        if (!isRight)
        {
            _dialogueContainer.offsetMin = new Vector2(50, _dialogueContainer.offsetMin.y);
            _leftSpeakerImg.color = new Color(_leftSpeakerImg.color.r, _leftSpeakerImg.color.g, _leftSpeakerImg.color.b, 1);
            if (isRevealed)
            {
                _leftMugCG.alpha = 1;
                if (isSpriteShowing)
                {
                    _dialogueContainer.offsetMin = new Vector2(350, _dialogueContainer.offsetMin.y);
                }
                else
                {
                    _leftSpeakerImg.color = new Color(_leftSpeakerImg.color.r, _leftSpeakerImg.color.g, _leftSpeakerImg.color.b, 0);
                }
            } else
            {
                _leftMugCG.alpha = 0;
            }
        } else
        {
            _dialogueContainer.offsetMax = new Vector2(-50, _dialogueContainer.offsetMax.y);
            _rightSpeakerImg.color = new Color(_rightSpeakerImg.color.r, _rightSpeakerImg.color.g, _rightSpeakerImg.color.b, 1);
            if (isRevealed)
            {
                _rightMugCG.alpha = 1;
                if (isSpriteShowing)
                {
                    _dialogueContainer.offsetMax = new Vector2(-350, _dialogueContainer.offsetMax.y);
                }
                else
                {
                    _rightSpeakerImg.color = new Color(_rightSpeakerImg.color.r, _rightSpeakerImg.color.g, _rightSpeakerImg.color.b, 0);
                }
            }
            else
            {
                _rightMugCG.alpha = 0;
            }
        }
    }

    // Refresh dialogue box (clear profile, text, etc.)
    private void Refresh()
    {
        RevealSpeaker(false, false);
        RevealSpeaker(true, false);
        dialogueSkipped = false;
        _dialogueContent.text = "";
    }

    private void SetSpeaker(DialogueData data)
    {
        Image targetImg;
        TMP_Text targetName;
        if (data.isRight)
        {
            targetImg = _rightSpeakerImg;
            targetName = _rightSpeakerName;
            RevealSpeaker(true, data.isSpriteShowing);
        } else
        {
            targetImg = _leftSpeakerImg;
            targetName = _leftSpeakerName;
            RevealSpeaker(false, data.isSpriteShowing);
        }
        targetImg.sprite = data.speakerSprite;
        targetName.text = data.speakerName;
    }

    // Prompt a dialogue sequence to happen
    public void RunDialogue(DialogueData data)
    {
        data.ImplementCharacter();
        Refresh();
        SetSpeaker(data);
        dialogueRunning = true;
        StartCoroutine(DialogueWriter(data));
    }

    public void PromptSequence(DialogueSequence sequence, CanvasGroup[] hidden = null)
    {
        if (sequence.sequence == null || sequence.sequence.Length < 1)
            return;
        // Hide UIs

        prevDialogueSkipped = false;
        sequenceRunning = true;
        sequence.sequence[0].ImplementCharacter();
        Refresh();
        SetSpeaker(sequence.sequence[0]);
        StartCoroutine(SequenceRunner(sequence, hidden));
    }

    private IEnumerator SequenceRunner(DialogueSequence sequence, CanvasGroup[] hidden)
    {
        while (_dialogueTransition.Progression < 1)
        {
            _dialogueTransition.Progress();
            _dialogueFrame.alpha = _dialogueTransition.Progression;
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < sequence.sequence.Length; i++)
        {
            RunDialogue(sequence.sequence[i]);
            while (dialogueRunning)
                yield return new WaitForSeconds(0.05f);
        }

        while (_dialogueTransition.Progression > 0)
        {
            _dialogueTransition.Revert();
            _dialogueFrame.alpha = _dialogueTransition.Progression;
            yield return new WaitForEndOfFrame();
        }

        sequenceRunning = false;

        // Unhide UIs
    }

    private IEnumerator DialogueWriter(DialogueData data)
    {
        for (int j = 0; j < data.content.Length; j++)
        {
            _dialogueContent.text = "";
            dialogueSkipped = false;
            string dialogue = data.content[j];
            for (int i = 0; i < dialogue.Length; i++)
            {
                _dialogueContent.text += dialogue[i];

                if (!dialogueSkipped)
                {
                    if (!_inputManager.actions["Accept"].IsPressed() && prevDialogueSkipped)
                        prevDialogueSkipped = false;

                    if (data.canSkip && !prevDialogueSkipped)
                    {
                        dialogueSkipped = _inputManager.actions["Accept"].IsPressed();
                        prevDialogueSkipped = dialogueSkipped;
                    }
                    AudioManager.Instance.PlaySFXInScreen("TextSFX");
                    yield return new WaitForSeconds(data.textSpeed);
                }
            }
            if (data.autoNext)
            {
                yield return new WaitForSeconds(data.waitTime);
            }
            else
            {
                _interactPrompt.SetActive(true);
                while (!_inputManager.actions["Accept"].WasPressedThisFrame())
                {
                    yield return new WaitForEndOfFrame();
                }
                _interactPrompt.SetActive(false);
            }
        }
        dialogueRunning = false;
    }

    private void BobInPr()
    {
        _interactPrompt.GetComponent<RectTransform>().position = new Vector2(_interactPrompt.GetComponent<RectTransform>().position.x, 
            _baseInPrPosition + (_bobInPrAmount * 0.5f - (_bobInPrAmount * Mathf.Sin(_bobInPrSpeed * Time.time))));
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _dialogueTransition = new Transition();
        _dialogueTransition.max = _transitionTime;
        _baseInPrPosition = _interactPrompt.GetComponent<RectTransform>().position.y;
        _inputManager = GetComponent<PlayerInput>();
    }

    // Update is called once per frame
    void Update()
    {
        BobInPr();
        DebugControls();
    }
}
