using UnityEngine;
using UnityEngine.InputSystem;

public class DGPlayer : DGEntity
{
    public System.Action<CHARACTER_STAT, int, int> OnLeaderStatChanged;
    private PlayerInput _inputManager;
    private CharacterBehaviour _cb;

    private new void Start()
    {
        base.Start();
        _inputManager = GetComponent<PlayerInput>();
        _cb = GetComponent<CharacterBehaviour>();
    }

    public bool CanControl
    {
        get { return !(_performingAction || _dgGameManager.CurrentEntityTurn() != this || _dungeonUI.menu.IsOpen); }
    }

    private new void Update()
    {
        base.Update();

        if (!CanControl) return;

        if (_inputManager.actions["Accept"].WasPressedThisFrame())
        {
            DefaultAttack.Instance.Perform(_cb);
            return;
        }

        int x, y;
        x = y = 0;

        if (_inputManager.actions["Up"].IsPressed())
        {
            y++;
        }
        if (_inputManager.actions["Right"].IsPressed())
        {
            x++;
        }
        if (_inputManager.actions["Down"].IsPressed())
        {
            y--;
        }
        if (_inputManager.actions["Left"].IsPressed())
        {
            x--;
        }

        Move(x, y);
    }
}
