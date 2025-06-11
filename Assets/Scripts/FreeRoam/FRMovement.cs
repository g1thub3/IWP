using UnityEngine;

public class FRMovement : MonoBehaviour
{
    // Assets
    protected Animator _animator;
    protected Rigidbody2D _rb;

    [Header("Stats")]
    [SerializeField] protected float _moveSpeed = 5.0f;

    protected string[][] anims;
    protected Vector2 _moveDir;
    protected int _direction;
    public int OppositeDirection
    {
        get { 
            switch (_direction)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
                default:
                    return 0;
            }
        }
    }

    protected void Start()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _moveDir = Vector2.zero;
        anims = new string[4][];
        anims[0] = new string[2];
        anims[0][0] = "idle_south";
        anims[0][1] = "walk_south";

        anims[1] = new string[2];
        anims[1][0] = "idle_west";
        anims[1][1] = "walk_west";

        anims[2] = new string[2];
        anims[2][0] = "idle_north";
        anims[2][1] = "walk_north";

        anims[3] = new string[2];
        anims[3][0] = "idle_east";
        anims[3][1] = "walk_east";
    }

    protected void Update()
    {
        if (_moveDir.x > 0)
        {
            _direction = 3;
        } 
        if (_moveDir.x < 0)
        {
            _direction = 1;
        }
        if (_moveDir.y > 0)
        {
            _direction = 2;
        }
        if (_moveDir.y < 0)
        {
            _direction = 0;
        }
        if (_moveDir.magnitude == 0)
        {
            _animator.Play(anims[_direction][0]);
        }
        else
        {
            _animator.Play(anims[_direction][1]);
        }

        var move = _moveDir * _moveSpeed * Time.deltaTime;
        _rb.position += move;
    }
}
