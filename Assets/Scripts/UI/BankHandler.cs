using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BankHandler : MonoBehaviour
{
    [SerializeField] CanvasGroup _group;
    [SerializeField] TMP_Text _bankMsg;
    [SerializeField] TMP_Text _bankAmt;
    [SerializeField] TMP_Text _walletAmt;
    [SerializeField] TMP_Text _inputAmt;
    [SerializeField] Image _arrowLeft, _arrowRight;

    private PromptInfo _bankPrompt;

    private PlayerInput _inputManager;
    private bool _isDepositing;
    private int _givenAmount;
    private bool _canClose;

    private int _digitIndex; // Max = 7

    private DialogueHandler _dialogueHandler;

    public bool IsOpen
    {
        get { return _group.alpha > 0; }
    }

    private void SetGivenAmount(int amt)
    {
        _givenAmount = Mathf.Clamp(amt, 0, GlobalGameManager.maxGold);
        Highlight();
    }

    private void ShiftIndex(int inc)
    {
        inc = Mathf.Clamp(inc, -1, 1);
        _digitIndex += inc;
        if (_digitIndex < 0)
            _digitIndex = 6;
        if (_digitIndex >= 7)
            _digitIndex = 0;
        Highlight();
    }

    private void Highlight()
    {
        string text = _givenAmount.ToString("0000000");
        string front = text.Substring(0, _digitIndex);
        string replaced = string.Format("<color=#f6ff80>{0}</color>", text[_digitIndex]);
        string back = text.Substring(_digitIndex + 1, text.Length - 1 - _digitIndex);
        string newText = string.Format("{0}{1}{2}",front,replaced,back);
        _inputAmt.text = newText;
    }

    public void Open(bool deposit)
    {
        _isDepositing = deposit;
        _arrowLeft.enabled = _isDepositing;
        _arrowRight.enabled = !_isDepositing;

        if (_isDepositing)
        {
            _bankMsg.text = "How much would you like to deposit?";
        } else
        {
            _bankMsg.text = "How much would you like to withdraw?";
        }

        _bankAmt.text = GlobalGameManager.Instance.bankGold.ToString();
        _walletAmt.text = GlobalGameManager.Instance.ownedGold.ToString();

        SetGivenAmount(0);
        _digitIndex = 6;
        ShiftIndex(0);

        _group.alpha = 1;
    }

    public void OpenPrompt()
    {
        GlobalCanvasManager.Instance.PromptHandler.Prompt(_bankPrompt);
        StartCoroutine(ProcessPrompt());
    }

    private IEnumerator ProcessPrompt()
    {
        while (GlobalCanvasManager.Instance.PromptHandler.IsInProgress())
        {
            yield return new WaitForEndOfFrame();
        }
        int ans = GlobalCanvasManager.Instance.PromptHandler.TakeAnswer();
        if (ans == 1)
        {
            Open(true);
        } else if (ans == 2)
        {
            Open(false);
        }
    }

    private void Close()
    {
        _canClose = true;
    }

    private void Start()
    {
        _inputManager = FindAnyObjectByType<PlayerInput>();
        _isDepositing = false;
        _givenAmount = 0;
        _digitIndex = 0;
        _dialogueHandler = GlobalCanvasManager.Instance.DialogueHandler;
        _canClose = false;

        _bankPrompt = new PromptInfo();
        _bankPrompt.message = "What would you like to do?";
        _bankPrompt.options = new string[3];
        _bankPrompt.options[0] = "Leave";
        _bankPrompt.options[1] = "Deposit Gold";
        _bankPrompt.options[2] = "Withdraw Gold";
    }

    private void Update()
    {
        if (IsOpen)
        {
            if (_canClose)
            {
                _group.alpha = 0;
                _canClose = false;
                return;
            }
            if (_inputManager.actions["Accept"].WasPressedThisFrame())
            {
                if (_isDepositing)
                {
                    _givenAmount = Mathf.Clamp(_givenAmount, 0, GlobalGameManager.Instance.ownedGold);
                    _givenAmount = Mathf.Min(_givenAmount, GlobalGameManager.Instance.BankCapacity);
                    GlobalGameManager.Instance.ownedGold -= _givenAmount;
                    GlobalGameManager.Instance.bankGold += _givenAmount;

                    string[] speech = {
                        _givenAmount + " Gold was deposited into the bank."
                    };
                    DialogueData data = new DialogueData(speech);
                    DialogueData[] sequence = { data };
                    _dialogueHandler.PromptSequence(sequence);

                } else
                {
                    _givenAmount = Mathf.Clamp(_givenAmount, 0, GlobalGameManager.Instance.bankGold);
                    _givenAmount = Mathf.Min(_givenAmount, GlobalGameManager.Instance.WalletCapacity);
                    GlobalGameManager.Instance.bankGold -= _givenAmount;
                    GlobalGameManager.Instance.ownedGold += _givenAmount;

                    string[] speech = {
                        _givenAmount + " Gold was withdrawn from the bank."
                    };
                    DialogueData data = new DialogueData(speech);
                    DialogueData[] sequence = { data };
                    _dialogueHandler.PromptSequence(sequence);
                }
                Close();
            }
            if (_inputManager.actions["Decline"].WasPressedThisFrame())
            {
                Close();
            }
            if (_inputManager.actions["Up"].WasPressedThisFrame())
            {
                SetGivenAmount(_givenAmount + (int)Mathf.Pow(10, 6 - _digitIndex));
            }
            if (_inputManager.actions["Down"].WasPressedThisFrame())
            {
                SetGivenAmount(_givenAmount - (int)Mathf.Pow(10, 6 - _digitIndex));
            }
            if (_inputManager.actions["Left"].WasPressedThisFrame())
            {
                ShiftIndex(-1);
            }
            if (_inputManager.actions["Right"].WasPressedThisFrame())
            {
                ShiftIndex(1);
            }
            if (_inputManager.actions["Anchor"].WasPressedThisFrame())
            {
                if (_givenAmount == 0)
                {
                    if (_isDepositing)
                    {
                        SetGivenAmount(GlobalGameManager.Instance.ownedGold);
                    } else
                    {
                        SetGivenAmount(GlobalGameManager.Instance.bankGold);
                    }
                } else
                {
                    SetGivenAmount(0);
                }
            }
        }
    }

}
