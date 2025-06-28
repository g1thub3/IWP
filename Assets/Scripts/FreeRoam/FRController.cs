using UnityEngine;
using UnityEngine.InputSystem;

public class FRController : FRMovement
{
    private QuestBoardHandler _questBoardHandler;
    private ShopStorageHandler _shopStorageHandler;
    private BankHandler _bankHandler;
    private CircleCollider2D _interactHitbox;
    private PlayerInput _inputManager;

    public bool CanControl
    {
        get {
            bool global = !GlobalCanvasManager.Instance.IsInteractionActive;
            bool questBoard = true;
            bool shopStor = true;
            bool bank = true;
            if (_questBoardHandler != null)
            {
                if (_questBoardHandler.IsOpen)
                    questBoard = false;
            }
            if (_shopStorageHandler != null)
            {
                if (_shopStorageHandler.IsOpen)
                    shopStor = false;
            }
            if (_bankHandler != null)
            {
                if (_bankHandler.IsOpen)
                    bank = false;
            }
            return global && questBoard && shopStor && bank;
        }
    }

    private new void Start()
    {
        base.Start();
        _inputManager = GetComponent<PlayerInput>();
        _interactHitbox = GetComponent<CircleCollider2D>();
        _questBoardHandler = FindAnyObjectByType<QuestBoardHandler>();
        _shopStorageHandler = FindAnyObjectByType<ShopStorageHandler>();
        _bankHandler = FindAnyObjectByType<BankHandler>();
    }

    private new void Update()
    {
        _moveDir = Vector2.zero;
        Move();

        base.Update();
        Interact();
        TriggerInteract();
    }

    private void Move()
    {
        if (!CanControl) return;
        if (_inputManager.actions["Up"].IsPressed())
            _moveDir.y = 1;
        if (_inputManager.actions["Down"].IsPressed())
            _moveDir.y = -1;
        if (_inputManager.actions["Left"].IsPressed())
            _moveDir.x = -1;
        if (_inputManager.actions["Right"].IsPressed())
            _moveDir.x = 1;
        if (_inputManager.actions["Anchor"].IsPressed())
            _moveDir *= 2;
    }

    private void Interact()
    {
        if (!CanControl) return;
        if (_inputManager.actions["Accept"].WasPressedThisFrame())
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, _interactHitbox.radius, LayerMask.GetMask("Interactable"));
            if (hit != null)
            {
                if (hit.TryGetComponent<FRInteractable>(out FRInteractable interactable))
                {
                    if (!interactable.interactOnTrigger)
                    {
                        interactable.OnInteract();
                    }
                    return;
                }
            }
        }
    }
    private void TriggerInteract()
    {
        if (!CanControl) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, _interactHitbox.radius, LayerMask.GetMask("Interactable"));
        if (hit != null)
        {
            if (hit.TryGetComponent<FRInteractable>(out FRInteractable interactable))
            {
                if (interactable.interactOnTrigger)
                {
                    interactable.OnInteract();
                }
                return;
            }
        }
    }
}
